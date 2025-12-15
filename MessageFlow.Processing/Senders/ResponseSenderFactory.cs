using MessageFlow.Processing.Senders.Abstractions;
namespace MessageFlow.Processing.Senders
{
    public class ResponseSenderFactory : IResponseSenderFactory
    {
        private readonly Dictionary<string, IResponseSender> _senders;

        public ResponseSenderFactory(IEnumerable<IResponseSender> senders)
        {
            _senders = senders.ToDictionary(
                s => s.ChannelType,
                StringComparer.OrdinalIgnoreCase);
        }

        public IResponseSender? Get(string channelType)
        {
            if (string.IsNullOrWhiteSpace(channelType))
                return null;

            return _senders.TryGetValue(channelType, out var sender)
                ? sender
                : null;
        }
    }
}
