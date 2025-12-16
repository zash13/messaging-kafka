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

        public async Task SendAsync(Envelope envelope, HandlerResult handlerResult, CancellationToken cancellationToken)
        {
            var sender = _serviceProvider.GetServices<IResponseSender>().FirstOrDefault(s => s.ChannelType.Equals(envelope.Channel, StringComparison.OrdinalIgnoreCase));
            if (sender is null)
                throw new InvalidOperationException($"No sender for channel '{envelope.Channel}'");
            await sender.SendAsync(envelope, handlerResult, cancellationToken);
        }
    }
}
