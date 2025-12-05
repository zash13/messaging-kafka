namespace MessageFlow.Kafka.Internals
{
    public interface IEnvelopeDataHelper
    {
        object CreatePayload(object data, Type payloadType);
    }
}

