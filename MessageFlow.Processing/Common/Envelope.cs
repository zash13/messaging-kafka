using System.Text.Json.Serialization;
namespace MessageFlow.Processing.Common
{
    public class Envelope
    {
        // I’ve been thinking about how to change this to a more reliable method,
        // instead of relying on a string (EventType). However, when you consider
        // cross-language scenarios—like a Python sender and a C# receiver—
        // along with other factors, it’s difficult for me to replace this part with anything else.
        [JsonPropertyName("envelope_type")]
        public string EnvelopeType { get; set; } = default!;

        [JsonPropertyName("channel_type")]
        public string ChannelType { get; set; } = default!;//telegram , web , other interfaces 

        [JsonPropertyName("event_version")]
        public int EventVersion { get; set; } = 1;

        [JsonPropertyName("aggregate_id")]
        public string AggregateId { get; set; } = default!;

        [JsonPropertyName("correlation_id")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }

        [JsonPropertyName("offset")]
        public long Offset { get; set; } = 0;

        [JsonPropertyName("data")]
        public object? Data { get; set; } // actual event payload
    }

}
