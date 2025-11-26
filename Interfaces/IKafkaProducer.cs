using Messaging.Kafka.Common;

namespace Messaging.Kafka.Interface
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
