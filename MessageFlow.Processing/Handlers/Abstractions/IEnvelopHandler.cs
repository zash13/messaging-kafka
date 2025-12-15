namespace MessageFlow.Processing.Handlers.Abstractions
{

    public interface IEnvelopeHandler<in TEvent>
    {
        Task<HandlerResult> HandleAsync(TEvent message, CancellationToken ct);
    }
}
