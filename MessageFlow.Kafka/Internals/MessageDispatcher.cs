using System.Text.Json;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Processing.Handlers.Abstractions;
using MessageFlow.Processing.Senders.Abstractions;
using MessageFlow.Processing.Common;
namespace MessageFlow.Kafka.Internals
{

    public class MessagDispatcher : IMessagDispatcher
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IEnvelopeRouter _router;
        private readonly IResponseSenderFactory _responseSender;
        public MessagDispatcher(IEnvelopeRouter router, IResponseSenderFactory responseSender, int maxConcurrency = 100)
        {
            _router = router;
            // no relase , immediatly enter all threads
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            _responseSender = responseSender;
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
                // this is the tricky part. i force myself to always send back a result for anything.
                // currently, i don’t see any cases where a result message isn’t needed.
                // however, if a scenario arises in the future, update handlerresult:
                // define a field and add a simple if-statement here.
                // avoid writing heavy logic in this section.
                var sender = _responseSender.Get(envelope.Channel);
                if (sender is null)
                {
                    Console.WriteLine("not sender found for this channel");
                    return DispatcherResutl.NoCommit();

                }
                await sender.SendAsync(envelope, handlerResult, cancellationToken);
                return handlerResult.ShouldCommit ? DispatcherResutl.Commit() : DispatcherResutl.NoCommit();
            }
            finally
            {
                _semaphore.Release();
            }
        }

    }
}
