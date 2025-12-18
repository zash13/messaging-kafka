using MessageFlow.Processing.Common;
namespace MessageFlow.Processing.Senders.Abstractions
{
    public interface IResponseSenderFactory
    {
        Task SendAsync(Dictionary<string, string>? metaData, string channel, HandlerResult handlerResult, CancellationToken cancellationToken);
    }
}
