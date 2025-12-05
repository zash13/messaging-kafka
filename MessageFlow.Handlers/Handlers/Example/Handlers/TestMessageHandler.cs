using Example.Domain;
using MessageFlow.Handlers.Abstractions;

namespace Example.Handler
{
    [EnvelopHandler("TestEvent")]
    public class TestMessageHandler : IEnvelopeHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage msg, CancellationToken ct)
        {
            Console.WriteLine("UserCreatedHandler received:");
            Console.WriteLine($"   name  {msg.Name}");
            Console.WriteLine($"   key : {msg.Key}");
            Console.WriteLine($"   datetiem  : {msg.Timestemp}");
            Console.WriteLine($"   list : {msg.TestList}");
            return Task.CompletedTask;
        }
    }
}
