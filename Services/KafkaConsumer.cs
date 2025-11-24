using System.Text.Json;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Messaging.Kafka.Services
{

    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;

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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
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

        public void StartConsumerLoop(CancellationToken cancellationToken)
        {
            Subscribe(this._topics);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume();
                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        try
                        {
                            var envelope = JsonSerializer.Deserialize<Envelope>(
                                consumeResult.Message.Value,
                                _jsonOptions
                            )!;
                            // place where handler will implement in future , 
                            // soprogramming work in progress 
                            _consumer.Commit(consumeResult);
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(
                                $"Processing failed for message: {consumeResult.Message.Value}", ex
                            );
                        }
                    }
                    else return;
                }
                catch (ConsumeException e)
                {
                    throw new ApplicationException(
                        $"Error consuming message: {e.Error.Reason}"
                    );
                }
            }
        }
        // override dispose from BackgroundService 
        // i mean this come from BackgroundService not IDispose 
        public override void Dispose()
        {
            _consumer?.Dispose();
        }
    }

}
