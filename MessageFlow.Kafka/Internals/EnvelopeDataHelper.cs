// This class helps the router generate clean data for the handler.
// i dont like this class , this ment to use as just json desrializer but now it act like mapping system for me 
using System.Text.Json;
using MessageFlow.Processing.Common;
namespace MessageFlow.Kafka.Internals
{

    public class EnvelopeDataHelper : IEnvelopeDataHelper
    {
        private readonly JsonSerializerOptions _options;

        public EnvelopeDataHelper(JsonSerializerOptions options)
        {
            _options = options;
        }

        public object Mapping(object data, Type payloadType)
        {
            var json = JsonSerializer.Serialize(data, _options);

            return JsonSerializer.Deserialize(json, payloadType, _options)
                   ?? throw new InvalidOperationException("Payload deserialization failed");
        }

        public T? Mapping<T>(object data)
        {
            var json = JsonSerializer.Serialize(data, _options);
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public T? MapMetadata<T>(Dictionary<string, string>? metadata) where T : class
        {
            if (metadata == null) return null;
            return Mapping<T>(metadata);
        }

        public T? MapPayload<T>(object? payload) where T : class
        {
            if (payload == null) return null;
            return Mapping<T>(payload);
        }

    }
}
