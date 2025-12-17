// This class helps the router generate clean data for the handler.
// I could add this logic to the handler , like a helper or something , but I don’t think it should work with raw JSON.
// The router itself should not be responsible for deserializing JSON—that’s not its job.
// Therefore, this helper exists to assist the router.
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

        public object CreatePayload(object data, Type payloadType)
        {
            var json = JsonSerializer.Serialize(data, _options);

            return JsonSerializer.Deserialize(json, payloadType, _options)
                   ?? throw new InvalidOperationException("Payload deserialization failed");
        }

    }
}
