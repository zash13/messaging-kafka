using MessageFlow.Processing.Common;
using MessageFlow.Processing.Senders.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MessageFlow.Processing.Senders
{
    public class ResponseSenderFactory : IResponseSenderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ResponseSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendAsync(
            Dictionary<string, string> metaData,
            string channel,
            HandlerResult handlerResult,
            CancellationToken cancellationToken
        )
        {
            // create a scope for scoped services , read more about root provider
            using var scope = _serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var allSenders = scopedProvider.GetServices<IResponseSender>().ToList();

            var matchedSender = allSenders.FirstOrDefault(s =>
                s.ChannelType.Equals(channel, StringComparison.OrdinalIgnoreCase)
            );

            if (matchedSender is null)
            {
                Console.WriteLine($"ERROR: No sender for channel '{channel}'");
                throw new InvalidOperationException($"No sender for channel '{channel}'");
            }

            await matchedSender.SendAsync(metaData, handlerResult, cancellationToken);
        }
    }
}
