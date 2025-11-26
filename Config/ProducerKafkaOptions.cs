namespace Messaging.Kafka.Config
{
    public class ProducerKafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string ProducerClientId { get; set; } = "dotnet-producer";
        public string DefaultTopic { get; set; } = "events-topic";

        public bool EnableIdempotence { get; set; } = true;
        public string Acks { get; set; } = "all";

        public int MessageSendMaxRetries { get; set; } = 5;
        public int LingerMs { get; set; } = 5;
        public string CompressionType { get; set; } = "none";
    }
}
