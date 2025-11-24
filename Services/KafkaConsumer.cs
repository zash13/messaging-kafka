using System.Text.Json;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


namespace Messaging.Kafka.Services
{

    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private readonly ConsumerKafkaOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };
        public KafkaConsumer(IOptions<ConsumerKafkaOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _topics = _options.Topics ?? throw new InvalidOperationException("Topics must be configured in ConsumerKafkaOptions");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                EnableAutoCommit = _options.EnableAutoCommit,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_options.AutoOffsetReset, true),
                EnablePartitionEof = _options.EnablePartitionEof,
                SessionTimeoutMs = _options.SessionTimeoutMs,
                HeartbeatIntervalMs = _options.HeartbeatIntervalMs,
                MaxPollIntervalMs = _options.MaxPollIntervalMs,
                QueuedMinMessages = _options.QueuedMinMessages,
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
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
