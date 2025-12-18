using MessageFlow.Processing.Common;
public interface IResponseSender
{
    string ChannelType { get; }

    // im not going to wrap these two small pieces into another class just to use them
    // forget creating extra classes for this — hopefully it works as is
    Task SendAsync(
        Dictionary<string, string> metaData,
        HandlerResult handlerResult,
        CancellationToken ct);
}
