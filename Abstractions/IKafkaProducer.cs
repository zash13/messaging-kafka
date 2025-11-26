

namespace MessageFlow.Kafka.Abstractions
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(
            string topic,
            string eventType,
            IQueueMessage eventMessage,
            string? correlationId = null
        );
    }
}
