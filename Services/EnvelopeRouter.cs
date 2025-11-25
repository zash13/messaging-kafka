using Messaging.Kafka.Common;
using System.Reflection;
using Messaging.Kafka.Interface;
using Messaging.Kafka.Interface;


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
        var handlerType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t =>
                t.GetCustomAttribute<EnvelopHandlerAttribute>()?.EnvelopType == envelope.EnvelopeType);

        if (handlerType == null)
            throw new Exception($"No handler found for event type: {envelope.EnvelopeType}");

        var handler = _provider.GetService(handlerType);
        if (handler == null)
            throw new Exception($"Handler type {handlerType.Name} not registered");

        // here i’m trying to cast object? data to your Data model type, which inherits from IEnvelopeData.
        // it’s important to create your model using this interface.
        // additionally, your handler should inherit from IEnvelopeHandler<YourDataModel>
        // where YourDataModel implements IEnvelopeData.
        var interfaceType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeHandler<>));
        var payloadType = interfaceType.GetGenericArguments()[0];

        // null .Data may break things here!
        var payload = _dataHelper.CreatePayload(envelope.Data, payloadType);

        var method = interfaceType.GetMethod("HandleAsync");
        await (Task)method.Invoke(handler, new[] { payload })!;
    }
}
