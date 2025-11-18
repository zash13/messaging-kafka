using Confluent.Kafka;

namespace Messaging.Kafka.Services
{
    public interface IKafkaConsumer
    {
        /// <summary>
        /// Register handler for raw consumed messages.
        /// </summary>
        void OnMessageReceived(Func<ConsumeResult<string, string>, Task> handler);
    }
}
