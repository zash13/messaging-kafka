namespace Messaging.Kafka.Config
{
    public class ConsumerKafkaOptions
    {
        public string BootstrapServers { get; set; } = "";
        public string GroupId { get; set; } = "";

        public bool EnableAutoCommit { get; set; } = false;
        public string AutoOffsetReset { get; set; } = "Earliest";

        public List<string> Topics { get; set; } = new List<string>();

        public bool EnableDlq { get; set; } = false;
        public string DlqTopicSuffix { get; set; } = "-dlq";
    }
}
