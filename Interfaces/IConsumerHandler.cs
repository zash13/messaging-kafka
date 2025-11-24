using Confluent.Kafka;
using Messaging.Kafka.Common;

namespace Messaging.Kafka.Services
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
