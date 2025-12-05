namespace MessageFlow.Kafka.Internals
{
    public interface IMessageDispatcher
    {
        Task DispatchAsync(Confluent.Kafka.ConsumeResult<string, string> result, CancellationToken cancellationToken);
    }
}
