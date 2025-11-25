using System.Text.Json;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Messaging.Kafka.Interface;


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

            var cfg = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = _options.ProducerClientId,
                Acks = _options.Acks?.ToLower() switch
                {
                    "all" => Acks.All,
                    "leader" => Acks.Leader,
                    "none" or "0" => Acks.None,
                    "1" => Acks.Leader, // "1" typically means leader acknowledgment
                    _ => Acks.All // default fallback
                },
                EnableIdempotence = _options.EnableIdempotence,
                MessageSendMaxRetries = _options.MessageSendMaxRetries,
                LingerMs = _options.LingerMs,
                CompressionType = _options.CompressionType?.ToLower() switch
                {
                    "none" => CompressionType.None,
                    "gzip" => CompressionType.Gzip,
                    "snappy" => CompressionType.Snappy,
                    "lz4" => CompressionType.Lz4,
                    "zstd" => CompressionType.Zstd,
                    _ => CompressionType.None // default fallback
                }
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

        public void StartConsumerLoop(CancellationToken cancellationToken)
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
                                var envelope = JsonSerializer.Deserialize<Envelope>(result.Message.Value);

                                // this is where i need to call router , but nothing happen for now  
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
