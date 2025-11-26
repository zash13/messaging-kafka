using System.Text.Json;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


namespace Messaging.Kafka.Services
{

    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private readonly ConsumerKafkaOptions _options;
        private readonly EnvelopeRouter _router;
        private readonly JsonSerializerOptions _jsonOptions;
        public KafkaConsumer(IOptions<ConsumerKafkaOptions> optionsAccessor, EnvelopeRouter envelopeRouter, JsonSerializerOptions jsonOptions)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _topics = _options.Topics ?? throw new InvalidOperationException("Topics must be configured in ConsumerKafkaOptions");
            _router = envelopeRouter;
            _jsonOptions = jsonOptions;

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
            // do not create another thread over here 
            // BackgroundService is already a thread 
            StartConsumerLoop(stoppingToken);
            return Task.CompletedTask;
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

        public async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TrySubscribe();

                    Console.WriteLine("Kafka consumer subscribed. Waiting for messages...");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer.Consume(cancellationToken);

                            if (result is { IsPartitionEOF: false })
                            {
                                var rawMessage = result.Message.Value;

                                Console.WriteLine($"Raw message from Kafka: '{rawMessage}'");
                                var envelope = JsonSerializer.Deserialize<Envelope>(result.Message.Value);
                                // this is where i need to call router , but nothing happen for now  
                                await _router.RouteAsync(envelope);
                                _consumer.Commit(result);
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            Console.WriteLine($"Consume error: {ex.Error.Reason}");
                        }
                    }
                }
                catch (KafkaException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    Console.WriteLine("Topic not created yet, retrying in 5 seconds...");
                    Task.Delay(5000, cancellationToken);
                }
            }
        }
        private void TrySubscribe()
        {
            try
            {
                _consumer.Subscribe(_topics);

                Console.WriteLine("Subscribed OK");
            }
            catch (KafkaException e) when (e.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                throw;
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
