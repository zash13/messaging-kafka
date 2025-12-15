using MessageFlow.Handlers.Abstractions;
namespace MessageFlow.Kafka.Abstractions
{

    public interface IEnvelopeRouter
    {
        Task<HandlerResult> RouteAsync(Envelope envelope, CancellationToken cancellationToken);
    }
}
