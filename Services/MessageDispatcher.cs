using System.Text.Json;
using Messaging.Kafka.Common;
namespace Messaging.Kafka.Services
{
    public class MessagDispatcher
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly EnvelopeRouter _router;
        public MessagDispatcher(EnvelopeRouter router, int maxConcurrency = 100)
        {
            _router = router;
            // no relase , immediatly enter all threads
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }
        public async Task DispatchAsync(Confluent.Kafka.ConsumeResult<string, string> result, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            _ = Task.Run(() => ProcessMessageAsync(result, cancellationToken), cancellationToken);
        }
        private async Task ProcessMessageAsync(Confluent.Kafka.ConsumeResult<string, string> result, CancellationToken cancellationToken)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<Envelope>(result.Message.Value);
                if (envelope != null)
                    await _router.RouteAsync(envelope);
            }
            finally
            {
                _semaphore.Release();
            }

        }

    }
}
