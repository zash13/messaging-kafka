namespace Messaging.Kafka.Config
{
    public class ProducerKafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "default-group";
        public bool EnableAutoCommit { get; set; } = false;
        public string AutoOffsetReset { get; set; } = "Earliest";
        public string ProducerClientId { get; set; } = "dotnet-producer";
        public int DefaultPartitions { get; set; } = 3;
        public string DlqTopicSuffix { get; set; } = "-dlq";
        public bool EnableDlq { get; set; } = true;
    }
}
