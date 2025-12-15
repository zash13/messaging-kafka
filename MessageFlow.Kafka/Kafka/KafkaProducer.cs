using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using MessageFlow.Kafka.Configuration;
using Microsoft.Extensions.Options;
using MessageFlow.Kafka.Internals;
using MessageFlow.Processing.Common;

namespace MessageFlow.Kafka
{

    public class KafkaProducer : IDisposable, IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly ProducerKafkaOptions _options;
        private readonly ISerializer _serializer;

        public KafkaProducer(ISerializer serializer, IOptions<ProducerKafkaOptions> optionAccessor)
        {
            _serializer = serializer ?? throw new ArgumentException(nameof(serializer));
            _options = optionAccessor.Value ?? throw new ArgumentException(nameof(optionAccessor));

            // this method of using configs is wrong , iwill change it 
            var cfg = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = _options.ProducerClientId,
                Acks = _options.Acks?.ToLower() switch
                {
                    "all" => Acks.All,
                    _ => Acks.All
                },
                EnableIdempotence = _options.EnableIdempotence,
                MessageSendMaxRetries = _options.MessageSendMaxRetries,
                LingerMs = _options.LingerMs,
                CompressionType = _options.CompressionType?.ToLower() switch
                {
                    "none" => CompressionType.None,
                    _ => CompressionType.None
                }
            };

            _producer = new ProducerBuilder<string, string>(cfg).Build();
        }

        public async Task ProduceAsync<T>(
            string topic, string envelopType, T message, string key, string? correlationId = null
            )
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException(nameof(topic));
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            var envelope = new Envelope()
            {
                EnvelopeType = envelopType,
                AggregateId = key,
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow,
                Data = message
            };
            var payload = _serializer.Serialize(envelope);
            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = payload
            };

            try
            {
                var result = await _producer.ProduceAsync(topic, kafkaMessage);
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Kafka produce error: {ex.Error.Reason}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {

                _producer.Flush(TimeSpan.FromSeconds(5));
            }
            catch { }
            _producer.Dispose();
        }

    }
}
