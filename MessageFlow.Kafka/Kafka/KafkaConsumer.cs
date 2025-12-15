using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MessageFlow.Kafka.Internals;
using MessageFlow.Kafka.Configuration;
using MessageFlow.Kafka.Abstractions;
using System.Threading.Tasks;

namespace MessageFlow.Kafka
{

    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private readonly IMessageDispatcher _messagDispatcher;

        public KafkaConsumer(IOptions<ConsumerKafkaOptions> optionsAccessor, IMessageDispatcher messagDispatcher)
        {
            var _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _topics = _options.Topics ?? throw new InvalidOperationException("Topics must be configured in ConsumerKafkaOptions");
            _messagDispatcher = messagDispatcher;
            if (_options.EnableAutoCommit == true)
                throw new NotSupportedException("AutoCommit is evables, consumer cannot work with that ");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                EnableAutoCommit = _options.EnableAutoCommit, // this should be false 
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
            return Task.Run(() => ConsumerLoop(stoppingToken), stoppingToken);
        }

        // this method is probably incorrect, but i’m not sure how to fix it.
        // both consuming and dispatching need to block the current thread .
        // consuming already blocks, but dispatching returns immediately if get a free thread
        // available in the semaphore.
        // suggestions for fixing this are welcome in here !, or anywhere ! 
        // for that , i change it from async task to void !!! 
        public async Task ConsumerLoop(CancellationToken cancellationToken)
        {
            TrySubscribe(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = _consumer.Consume(cancellationToken);

                }
                catch
                {
                    continue;
                }
                if (result == null || result.IsPartitionEOF)
                    continue;
                DispatcherResutl dispatcherResutl;
                try
                {
                    dispatcherResutl = await _messagDispatcher.DispatchAsync(result, cancellationToken);
                }
                catch
                {
                    continue;
                }
                //no cancellation token — commits must always complete if handler succeeded

                if (dispatcherResutl.ShouldCommit)
                    try { _consumer.Commit(result); }
                    catch
                    {//log }
                    }
            }
        }
        private void TrySubscribe(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Subscribe(_topics);
                    break;
                }
                catch (KafkaException e) when (e.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    throw;
                }
            }
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
        // override dispose from BackgroundService 
        // i mean this come from BackgroundService not IDispose 
        public override void Dispose()
        {
            _consumer?.Dispose();
        }
    }
}
