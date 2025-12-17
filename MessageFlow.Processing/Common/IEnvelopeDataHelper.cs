namespace MessageFlow.Processing.Common
{
    public interface IEnvelopeDataHelper
    {
        object CreatePayload(object data, Type payloadType);
    }
}

