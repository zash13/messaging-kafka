namespace MessageFlow.Processing.Common
{
    public interface IKafkaProducer
    {
        void Dispose();
        Task ProduceAsync<TContext, TPayload>(string topic, string eventType, string channel, TPayload payload, string key, TContext? context = null, string? correlationId = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default) where TContext : class, new();
        Task ProduceAsync<TPayload>(string topic, string eventType, string channel, TPayload payload, string key, object? context = null, string? correlationId = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
        Task ProduceAsync<TPayload>(string topic, string eventType, string channel, TPayload payload, string key, string? correlationId = null, CancellationToken cancellationToken = default);
    }
}
