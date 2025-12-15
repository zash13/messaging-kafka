namespace MessageFlow.Processing.Senders.Abstractions
{
    public interface IResponseSenderFactory
    {
        IResponseSender? Get(string channelType);

    }
}
