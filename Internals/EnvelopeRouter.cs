using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MessageFlow.Kafka.Abstractions;


namespace MessageFlow.Kafka.Internals
{

    public class EnvelopeRouter
    {
        private readonly IServiceProvider _provider;
        private readonly IEnvelopeDataHelper _dataHelper;

        public EnvelopeRouter(IServiceProvider provider, IEnvelopeDataHelper dataHelper)
        {
            _provider = provider;
            _dataHelper = dataHelper;
        }
        public async Task RouteAsync(Envelope envelope)
        {
            if (envelope == null) return;

            var handlerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException) { return Type.EmptyTypes; }
                })
                .FirstOrDefault(t => t.GetCustomAttribute<EnvelopHandlerAttribute>()?.EnvelopType == envelope.EnvelopeType);

            if (handlerType == null) return;

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
        static string TypeName(Type t) => t.FullName ?? t.Name;
    }
}
