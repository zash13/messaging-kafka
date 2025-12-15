using System.Text.Json;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Handlers.Abstractions;
using Confluent.Kafka;
namespace MessageFlow.Kafka.Internals
{

    public class MessagDispatcher : IMessagDispatcher
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IEnvelopeRouter _router;
        public MessagDispatcher(IEnvelopeRouter router, int maxConcurrency = 100)
        {
            _router = router;
            // no relase , immediatly enter all threads
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }
        public async Task<DispatcherResutl> DispatchAsync(Confluent.Kafka.ConsumeResult<string, string> result, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            // no cancellationToken , you will loss message then in flight!!! 
            try
            {
                Envelope? envelope;
                try
                {

                    envelope = JsonSerializer.Deserialize<Envelope>(result.Message.Value);
                }
                catch
                {
                    // bad envelop , i remove it from queue 
                    return DispatcherResutl.Commit();
                }
                // empty or what emvelop , still commit  
                if (envelope == null)
                    return DispatcherResutl.Commit();
                HandlerResult handlerResult;
                try
                {
                    handlerResult = await _router.RouteAsync(envelope, cancellationToken);
                }
                catch
                {
                    // something unexcepted happen ,so no commit ( message is healthy ) here i am sure that if message processed in any step and failed , i have a result not exception 
                    return DispatcherResutl.NoCommit();
                }
                return handlerResult.ShouldCommit ? DispatcherResutl.Commit() : DispatcherResutl.NoCommit();
                // this is the tricky part. i force myself to always send back a result for anything.
                // currently, i don’t see any cases where a result message isn’t needed.
                // however, if a scenario arises in the future, update handlerresult:
                // define a field and add a simple if-statement here.
                // avoid writing heavy logic in this section.
                PublisherFactory();

            }
            finally
            {
                _semaphore.Release();
            }
        }

    }
}
