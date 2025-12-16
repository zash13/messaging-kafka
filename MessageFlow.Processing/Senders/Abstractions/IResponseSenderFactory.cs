using MessageFlow.Processing.Common;
namespace MessageFlow.Processing.Senders.Abstractions
{
    public interface IResponseSenderFactory
    {
        Task SendAsync(Envelope envelope, HandlerResult handlerResult, CancellationToken cancellationToken);
    }
}
