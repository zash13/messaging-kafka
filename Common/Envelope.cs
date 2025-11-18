using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Messaging.Kafka.Common
{
    public class Envelope
    {
        public string EventType { get; set; } = default!;
        public int EventVersion { get; set; } = 1;
        public string AggregateId { get; set; } = default!;
        public string? CorrelationId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public Dictionary<string, string>? Metadata { get; set; }
        public object? Data { get; set; } // actual event payload
    }
}
