

namespace MessageFlow.Kafka.Abstractions
{


    public interface IKafkaProducer
    {
        void Dispose();
        Task ProduceAsync<T>(string topic, string envelopType, T message, string key, string? correlationId = null);
    }
}
