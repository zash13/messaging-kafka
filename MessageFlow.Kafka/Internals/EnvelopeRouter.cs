// This class is not solid. It definitely lacks a single responsibility and is not safe.
// If something goes wrong with your project, the issue could be here!
// not mention threading is not properly implementd yet !!!!! 
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Handlers.Abstractions;


namespace MessageFlow.Kafka.Internals
{

    public class EnvelopeRouter : IEnvelopeRouter
    {
        private readonly IServiceProvider _provider;
        private readonly IEnvelopeDataHelper _dataHelper;
        private readonly Dictionary<string, Type> _handlerMap;

        public EnvelopeRouter(IServiceProvider provider, IEnvelopeDataHelper dataHelper, Dictionary<string, Type> map)
        {
            _provider = provider;
            _dataHelper = dataHelper;
            _handlerMap = map;
        }
        public async Task<HandlerResult> RouteAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
                return HandlerResult.ServerFailuer(
                    serverMessage: "Envelope is null",
                    userMessage: "Server error "
                );

            if (!_handlerMap.TryGetValue(envelope.EnvelopeType, out var handlerType))
                return HandlerResult.ServerFailuer(
                    serverMessage: $"No handler found for envelope type: {envelope.EnvelopeType}",
                    userMessage: "Server error "
                );

            using var scope = _provider.CreateScope();
            var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);

            var interfaceType = handlerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeHandler<>));

            if (interfaceType == null)
                return HandlerResult.ServerFailuer(
                    serverMessage: $"Handler does not implement IEnvelopeHandler<> or server cannot create instanse ",
                    userMessage: "Server error "
                );

            var payloadType = interfaceType.GetGenericArguments()[0];
            var payload = _dataHelper.CreatePayload(envelope.Data, payloadType);

            if (payload == null)
                return HandlerResult.ValidationError(
                    serverMessage: "Failed to create payload from envelope data",
                    userMessage: "Server error ",
                    validationErrors: new
                    {
                        EnvelopeType = envelope.EnvelopeType,
                        ExpectedType = payloadType.Name
                    }
                );

            var method = interfaceType.GetMethod("HandleAsync");
            try
            {
                if (method == null)
                    return HandlerResult.ServerFailuer(
                        serverMessage: "HandleAsync method not found on handler",
                        userMessage: "Handler configuration error"
                    );

                var task = (Task<HandlerResult>)method.Invoke(handler, new object[] { payload, cancellationToken });
                return await task;

            }
            catch (OperationCanceledException)
            {
                return HandlerResult.ServerFailuer(serverMessage: "operation was cancelled ");
            }
            catch (Exception ex)
            {

                return HandlerResult.ServerFailuer(serverMessage: $"operation fauild : {ex} ");
            }
        }
    }
}
