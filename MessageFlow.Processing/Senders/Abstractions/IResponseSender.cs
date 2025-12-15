using MessageFlow.Processing.Common;
public interface IResponseSender
{
    string ChannelType { get; }

    Task SendAsync(
        Envelope envelope,
        HandlerResult handlerResult,
        CancellationToken ct);
}
