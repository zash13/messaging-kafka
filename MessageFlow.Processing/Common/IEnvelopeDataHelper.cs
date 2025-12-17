namespace MessageFlow.Processing.Common
{
    public interface IEnvelopeDataHelper
    {
        T? MapMetadata<T>(Dictionary<string, string>? metadata) where T : class;
        T? MapPayload<T>(object? payload) where T : class;
        object Mapping(object data, Type payloadType);
        T? Mapping<T>(object data);
    }
}

