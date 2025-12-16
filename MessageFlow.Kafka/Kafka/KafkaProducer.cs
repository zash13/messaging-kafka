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

            var cfg = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = _options.ProducerClientId,
                // this probably wrong , read more about acks
                Acks = _options.Acks?.ToLower() switch
                {
                    "all" => Acks.All,
                    _ => Acks.All
                },
                EnableIdempotence = _options.EnableIdempotence,
                MessageSendMaxRetries = _options.MessageSendMaxRetries,
                LingerMs = _options.LingerMs,
                // this is also is not complite , read more about Compressions
                CompressionType = _options.CompressionType?.ToLower() switch
                {
                    "none" => CompressionType.None,
                    _ => CompressionType.None
                }
            };

            _producer = new ProducerBuilder<string, string>(cfg).Build();
        }

        public async Task ProduceAsync<TPayload>(
            string topic,
            string eventType,
            string channel,
            TPayload payload,
            string key,
            string? correlationId = null,
            Dictionary<string, string>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(topic, eventType, channel, payload, key);

            var envelope = CreateEnvelope(eventType, channel, payload, correlationId, metadata);
            await ProduceToKafkaAsync(topic, key, envelope, cancellationToken);
        }

        public async Task ProduceAsync<TPayload>(
            string topic,
            string eventType,
            string channel,
            TPayload payload,
            string key,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(topic, eventType, channel, payload, key);

            var envelope = CreateEnvelope(eventType, channel, payload, correlationId, null);
            await ProduceToKafkaAsync(topic, key, envelope, cancellationToken);
        }
        #region helper methods 

        private void ValidateParameters<T>(string topic, string eventType, string channel, T payload, string key)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        private Envelope CreateEnvelope<TPayload>(
            string eventType,
            string channel,
            TPayload payload,
            string? correlationId,
            Dictionary<string, string>? metadata)
        {
            return new Envelope
            {
                EventType = eventType,
                Channel = channel,
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow,
                Payload = payload,
                Metadata = metadata
            };
        }

        private async Task ProduceToKafkaAsync(
            string topic,
            string key,
            Envelope envelope,
            CancellationToken cancellationToken)
        {
            try
            {
                var payload = _serializer.Serialize(envelope);
                var kafkaMessage = new Message<string, string>
                {
                    Key = key,
                    Value = payload,
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Kafka produce error: {ex.Error.Reason}");
                throw;
            }
        }

        #endregion
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
