namespace Messaging.Kafka.Interface
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent message, CancellationToken ct);
    }
}
