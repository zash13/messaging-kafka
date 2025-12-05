namespace MessageFlow.Handlers.Abstractions
{

    public interface IEnvelopeHandler<in TEvent>
    {
        Task HandleAsync(TEvent message, CancellationToken ct);
    }
}
