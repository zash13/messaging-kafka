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
        public async Task RouteAsync(Envelope envelope)
        {
            if (envelope == null) return;
            if (!_handlerMap.TryGetValue(envelope.EnvelopeType, out var handlerType)) return;

            var handler = ActivatorUtilities.CreateInstance(_provider, handlerType);

            // here i’m trying to cast object? data to your Data model type, which inherits from IEnvelopeData.
            // it’s important to create your model using this interface.
            // additionally, your handler should inherit from IEnvelopeHandler<YourDataModel>
            // where YourDataModel implements IEnvelopeData.
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeHandler<>));

            var payloadType = interfaceType.GetGenericArguments()[0];
            var payload = _dataHelper.CreatePayload(envelope.Data, payloadType);

            if (payload == null) return;

            var method = interfaceType.GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, new object[] { payload, CancellationToken.None })!;
        }
    }
}
