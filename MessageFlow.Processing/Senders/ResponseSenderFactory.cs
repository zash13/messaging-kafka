using Microsoft.Extensions.DependencyInjection;
using MessageFlow.Processing.Common;
using MessageFlow.Processing.Senders.Abstractions;
namespace MessageFlow.Processing.Senders
{

    public class ResponseSenderFactory : IResponseSenderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ResponseSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendAsync(Dictionary<string, string> metaData, string channel, HandlerResult handlerResult, CancellationToken cancellationToken)
        {
            var sender = _serviceProvider.GetServices<IResponseSender>().FirstOrDefault(s => s.ChannelType.Equals(channel, StringComparison.OrdinalIgnoreCase));
            if (sender is null)
                throw new InvalidOperationException($"No sender for channel '{channel}'");
            await sender.SendAsync(metaData, handlerResult, cancellationToken);
        }
    }
}
