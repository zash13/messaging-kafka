// read this repo
// https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/Consumer/Program.cs
using System.Text.Json;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Configuration;

namespace Messaging.Kafka.Services
{

    public class KafkaConsumer : IKafkaConsumer, IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        // OR (if you want to read the key later)
        // private readonly IConsumer<string, string> _consumer;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };
        public KafkaConsumer()
        {
            var configPath = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "Messaging.Kafka",
                    "consumer.config.json"
                )
            );
            Console.WriteLine(configPath.ToString());

            var config = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: false)
                .Build();
            var options =
                config.GetSection("ConsumerKafkaOptions").Get<ConsumerKafkaOptions>()
                ?? throw new Exception("Failed to load consumer.config.json");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = options.GroupId,
                EnableAutoCommit = options.EnableAutoCommit,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(options.AutoOffsetReset, true),
                SessionTimeoutMs = options.SessionTimeoutMs,
                HeartbeatIntervalMs = options.HeartbeatIntervalMs,
                MaxPollIntervalMs = options.MaxPollIntervalMs,
                QueuedMinMessages = options.QueuedMinMessages,
                EnablePartitionEof = options.EnablePartitionEof,
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).SetValueDeserializer(Deserializers.Utf8).Build();
        }
        private void Subscribe(IEnumerable<string> topics)
        {
            _consumer.Subscribe(topics);
        }
        private void Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
        }
        private void Unsubscribe() => _consumer.Unsubscribe();
        public void ConsumeSingleMessage(string topic)
        {
            Subscribe(topic);
            try
            {
                var consumeResult = _consumer.Consume();
                if (consumeResult != null && !consumeResult.IsPartitionEOF)
                {
                    try
                    {
                        Console.WriteLine(consumeResult.Message.ToString());
                        var envelope = JsonSerializer.Deserialize<Envelope>(
                            consumeResult.Message.Value,
                            _jsonOptions
                        )!;

                        Console.WriteLine(
                            $"Event: {envelope.EventType} | Key: {consumeResult.Message.Key} | Aggregate: {envelope.AggregateId}"
                        );
                        Console.WriteLine($"envelope sutff : {envelope.Data.ToString()}");

                        _consumer.Commit(consumeResult);
                        Console.WriteLine("message committed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Processing failed → {ex.Message}");
                        // No commit → message will be retried
                    }
                    Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");
                }
                else
                {
                    Console.WriteLine("No message available or reached end of partition.");
                }
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Error consuming message: {e.Error.Reason}");
            }
        }
        public void ConsumeMessages(string topic)
        {
            Subscribe(topic);

            try
            {
                while (true)
                {
                    var consumeResult = _consumer.Consume();
                    try
                    {
                        Console.WriteLine(consumeResult.Message.ToString());
                        var envelope = JsonSerializer.Deserialize<Envelope>(
                            consumeResult.Message.Value,
                            _jsonOptions
                        )!;

                        Console.WriteLine(
                            $"Event: {envelope.EventType} | Key: {consumeResult.Message.Key} | Aggregate: {envelope.AggregateId}"
                        );

                        _consumer.Commit(consumeResult);
                        Console.WriteLine("message commited successfully ");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Processing failed → {ex.Message}");
                        // No commit → message will be retried
                    }
                    Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");
                }
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Error consuming message: {e.Error.Reason}");
            }
        }
        public void Dispose()
        {
            _consumer?.Dispose();
        }
    }

}
