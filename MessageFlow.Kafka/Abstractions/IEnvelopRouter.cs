using MessageFlow.Processing.Common;
namespace MessageFlow.Kafka.Abstractions
{

    public interface IEnvelopeRouter
    {
        Task<HandlerResult> RouteAsync(Envelope envelope, CancellationToken cancellationToken);
    }
}
