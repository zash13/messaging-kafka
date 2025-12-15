using MessageFlow.Kafka.Abstractions;
using Confluent.Kafka;


namespace MessageFlow.Kafka.Internals
{
    public interface IMessagDispatcher
    {
        Task<DispatcherResutl> DispatchAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken);
    }
}
