using MessageFlow.Processing.Common;
namespace MessageFlow.Kafka.Abstractions
{

    public interface IEnvelopeRouter
    {
        Task<HandlerResult> RouteAsync(string eventType, object data, CancellationToken cancellationToken);
    }
}
