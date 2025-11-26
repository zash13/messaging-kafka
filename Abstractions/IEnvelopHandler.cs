namespace MessageFlow.Kafka.Abstractions
{

    public interface IEnvelopeHandler<in TEvent>
    {
        Task HandleAsync(TEvent message, CancellationToken ct);
    }
}
