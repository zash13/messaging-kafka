using System;
using Confluent.Kafka;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Messaging.Kafka.Services
{
    public class KafkaConsumer : BackgroundService, IKafkaConsumer, IDisposable
    {
        private readonly ConsumerKafkaOptions _options;
        private readonly ISerializer _serializer;
        private readonly IConsumer<string, string> _consumer;

        private Func<ConsumeResult<string, string>, Task>? _onMessageReceived;

        public KafkaConsumer(IOptions<ConsumerKafkaOptions> options, ISerializer serializer)
        {
            _options = options.Value;
            _serializer = serializer;

            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                EnableAutoCommit = false,
                AutoOffsetReset = Enum.TryParse<AutoOffsetReset>(
                    _options.AutoOffsetReset,
                    true,
                    out var r
                )
                    ? r
                    : AutoOffsetReset.Earliest,
                EnablePartitionEof = false,
                SessionTimeoutMs = 6000,
                StatisticsIntervalMs = 5000,
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetStatisticsHandler((_, statsJson) => LogStats(statsJson))
                .SetErrorHandler((_, err) => LogError(err))
                .Build();
        }

        public void OnMessageReceived(Func<ConsumeResult<string, string>, Task> handler)
        {
            _onMessageReceived = handler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_options.Topics);

            return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
        }

        private async Task ConsumeLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(ct);
                    if (result == null || result.Message == null)
                        continue;

                    if (_onMessageReceived != null)
                        await _onMessageReceived(result);

                    _consumer.Commit(result);
                }
                catch (ConsumeException ce)
                {
                    Console.WriteLine($"[Kafka] Consume Error: {ce.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    break; // shutdown
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Kafka] General Error: {ex}");
                }
            }
        }

        private void LogStats(string statsJson)
        {
            try
            {
                var stats = JsonConvert.DeserializeObject<dynamic>(statsJson);
                Console.WriteLine($"[Kafka] Stats: {statsJson}");
            }
            catch { }
        }

        private void LogError(Error err)
        {
            Console.WriteLine($"[Kafka] Error: {err.Code} - {err.Reason}");
        }

        public override void Dispose()
        {
            try
            {
                _consumer.Close();
                _consumer.Dispose();
            }
            catch { }
            base.Dispose();
        }
    }
}
