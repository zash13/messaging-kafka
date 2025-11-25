namespace Messaging.Kafka.Interface
{
    public interface IEnvelopeDataHelper
    {
        object CreatePayload(object data, Type payloadType);
    }
}

