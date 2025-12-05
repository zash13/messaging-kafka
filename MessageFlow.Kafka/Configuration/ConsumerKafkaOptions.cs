namespace MessageFlow.Kafka.Configuration
{
    public class ConsumerKafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "default-group";
        public bool EnableAutoCommit { get; set; } = false;
        public string AutoOffsetReset { get; set; } = "earliest";
        public string[] Topics { get; set; } = Array.Empty<string>();
        public int SessionTimeoutMs { get; set; } = 10000;
        public int HeartbeatIntervalMs { get; set; } = 3000;
        public int MaxPollIntervalMs { get; set; } = 300000;
        public int QueuedMinMessages { get; set; } = 100000;
        public bool EnablePartitionEof { get; set; } = false;
    }
}
