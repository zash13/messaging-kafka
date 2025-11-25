namespace Messaging.Kafka.Common
{
    public class Envelope
    {
        // I’ve been thinking about how to change this to a more reliable method,
        // instead of relying on a string (EventType). However, when you consider
        // cross-language scenarios—like a Python sender and a C# receiver—
        // along with other factors, it’s difficult for me to replace this part with anything else.
        public string EnvelopeType { get; set; } = default!;

        public int EventVersion { get; set; } = 1;
        public string AggregateId { get; set; } = default!;
        public string? CorrelationId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public Dictionary<string, string>? Metadata { get; set; }
        public long Offset { get; set; } = 0;
        public object? Data { get; set; } // actual event payload

    }
}
