using System.Text.Json.Serialization;
namespace MessageFlow.Processing.Common
{
    public abstract class BaseEnvelope
    {
        // I’ve been thinking about how to change this to a more reliable method,
        // instead of relying on a string (EventType). However, when you consider
        // cross-language scenarios—like a Python sender and a C# receiver—
        // along with other factors, it’s difficult for me to replace this part with anything else.
        [JsonPropertyName("envelope_id")]
        public Guid EnvelopeId { get; set; } = Guid.NewGuid();

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = default!; // login ,shopcard ....

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = default!;//telegram , web , other interfaces or users 

        // i dont know where , but version may be usefull in future 
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("correlation_id")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;


    }
    public class Envelope : BaseEnvelope
    {
        // like region in web or message presentation property in telegram
        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
        // this is new alternative instend of object data 
        [JsonPropertyName("payload")]
        public object? Payload { get; set; }

    }

}
