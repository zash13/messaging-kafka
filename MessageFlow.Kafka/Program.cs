using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Example.Domain;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Kafka;
using MessageFlow.Kafka.Configuration;

class Program
{
    private static IHost? _host;
    private static bool _hostRunning = false;

    static async Task Main(string[] args)
    {
        try
        {
            _host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddKafkaEmbeddedConfigs();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddKafkaMessaging(context.Configuration);
                })
                .Build();

            var hostTask = _host.RunAsync();
            _hostRunning = true;

            var producer = _host.Services.GetRequiredService<IKafkaProducer>();
            const string topic = "topic1";

            Console.WriteLine($"Starting producer + consumer on topic: {topic}");
            Console.WriteLine("Note: Consumer may fail initially if topic doesn't exist - this is expected!");
            Console.WriteLine("Commands:");
            Console.WriteLine("  [1] - Send one message (this will create the topic)");
            Console.WriteLine("  [2] - Read one message");
            Console.WriteLine("  [3] - Send multiple messages (5)");
            Console.WriteLine("  [q] - Quit");
            Console.WriteLine();

            // Give the host a moment to start (consumer will fail but we ignore it)
            await Task.Delay(2000);
            Console.WriteLine("✅ Producer ready! Consumer may restart automatically when topic is created.");

            while (true)
            {
                Console.Write("Enter command (1/2/3/q): ");
                var input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "1":
                        await SendSingleMessage(producer, topic);
                        break;

                    case "2":
                        Console.WriteLine("Read message functionality not implemented in this version");
                        break;

                    case "3":
                        await SendMultipleMessages(producer, topic, 5);
                        break;

                    case "q":
                        Console.WriteLine("Exiting...");
                        _hostRunning = false;
                        await _host.StopAsync();
                        return;

                    default:
                        Console.WriteLine("Invalid command. Use 1, 2, 3, or q.");
                        break;
                }

                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Application error: {ex.Message}");
        }
        finally
        {
            if (_hostRunning && _host != null)
            {
                await _host.StopAsync();
            }
        }
    }

    private static async Task SendSingleMessage(IKafkaProducer producer, string topic)
    {
        try
        {
            var testEvent = new TestMessage
            {
                Id = Random.Shared.Next(1, 9999),
                Name = $"testname_{DateTime.Now:HHmmss}",
                TestList = new List<string> { "a", "b", "c", DateTime.Now.ToString("O") }
            };

            await producer.ProduceAsync<TestMessage>(
                topic: topic,
                envelopType: "TestEvent",
                message: testEvent,
                key: testEvent.Key,
                correlationId: Guid.NewGuid().ToString()
            );

            Console.WriteLine($"✅ SENT → {testEvent.Name} (ID: {testEvent.Id})");
            Console.WriteLine("💡 Note: After sending the first message, the topic is created and consumer should automatically recover!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send message: {ex.Message}");
        }
    }

    private static async Task SendMultipleMessages(IKafkaProducer producer, string topic, int count)
    {
        try
        {
            Console.WriteLine($"Sending {count} messages...");

            for (int i = 0; i < count; i++)
            {
                var testEvent = new TestMessage
                {
                    Id = Random.Shared.Next(1, 9999),
                    Name = $"batch_{i + 1}_{DateTime.Now:HHmmss}",
                    TestList = new List<string> { "batch", "message", $"#{i + 1}" }
                };

                await producer.ProduceAsync<TestMessage>(
                    topic: topic,
                    envelopType: "TestEvent",
                    message: testEvent,
                    key: testEvent.Key,
                    correlationId: Guid.NewGuid().ToString()
                );

                Console.WriteLine($"✅ SENT {i + 1}/{count} → {testEvent.Name} (ID: {testEvent.Id})");
                await Task.Delay(100);
            }

            Console.WriteLine($"✅ All {count} messages sent!");
            Console.WriteLine("💡 Note: Consumer should now be receiving these messages if it has recovered!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send messages: {ex.Message}");
        }
    }
}
/*
 * how web app should look like 
var builder = WebApplication.CreateBuilder(args);

// order is matter in here 
builder.Configuration.AddKafkaEmbeddedConfigs();
builder.Services.AddKafkaMessaging(builder.Configuration);

var app = builder.Build();
app.Run();
 */
