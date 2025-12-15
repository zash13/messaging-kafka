using Example.Domain;
using MessageFlow.Processing.Handlers.Abstractions;
using MessageFlow.Processing.Common;

namespace Example.Handler
{
    [EnvelopHandler("TestEvent")]
    public class TestMessageHandler : IEnvelopeHandler<TestMessage>
    {
        public async Task<HandlerResult> HandleAsync(TestMessage msg, CancellationToken ct)
        {
            Console.WriteLine("UserCreatedHandler received:");
            Console.WriteLine($"   name  {msg.Name}");
            Console.WriteLine($"   key : {msg.Key}");
            Console.WriteLine($"   datetiem  : {msg.Timestemp}");
            Console.WriteLine($"   list : {msg.TestList}");
            return HandlerResult.Success("Success");
        }
    }
}
