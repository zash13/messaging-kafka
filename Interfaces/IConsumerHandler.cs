using Confluent.Kafka;
using Messaging.Kafka.Common;

namespace Messaging.Kafka.Interface
{
    public interface IConsumerHandler
    {
        Task HandleAsync(
            Envelope envelope,
            ConsumeResult<string, string> meta,
            CancellationToken ct
        );
    }
}
