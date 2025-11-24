namespace Messaging.Kafka.Handlers
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent message, CancellationToken ct);
    }
}
