/// all event data types that will be sent to kafka must implement this.
/// the key is used as kafka partitioning key (hash(key) % partitions) and ordering
namespace Messaging.Kafka.Common
{
    public interface IQueueMessage
    {
        /// <summary>
        /// Partitioning/ordering key for Kafka.
        /// </summary>
        string Key { get; }
    }
}
