using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Options;

namespace Messaging.Kafka.Services
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ProducerKafkaOptions _options;
        private readonly ISerializer _serializer;

        public KafkaProducer(IOptions<ProducerKafkaOptions> options, ISerializer serializer)
        {
            _options = options.Value;
            _serializer = serializer;

            var config = new ProducerConfig
            {
                BootstrapServers = _options?.BootstrapServers,
                ClientId = _options.ProducerClientId,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 5,
                RetryBackoffMs = 100,
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync(
            string topic,
            string eventType,
            IQueueMessage @event,
            string? correlationId = null
        )
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException(nameof(topic));
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var envelope = new Envelope
            {
                EventType = eventType,
                AggregateId = @event.Key,
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow,
                Data = @event,
            };

            var json = _serializer.Serialize(envelope);

            var msg = new Message<string, string> { Key = @event.Key, Value = json };

            // Produce and wait for ack
            var result = await _producer.ProduceAsync(topic, msg).ConfigureAwait(false);
            // We don't use logger; user may want to inspect result in debugging
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}
