namespace MessageFlow.Processing.Common
{
    public interface IKafkaProducer
    {
        void Dispose();
        // with or without meta data 
        Task ProduceAsync<TPayload>(string topic, string eventType, string channel, TPayload payload, string key, string? correlationId = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
        Task ProduceAsync<TPayload>(string topic, string eventType, string channel, TPayload payload, string key, string? correlationId = null, CancellationToken cancellationToken = default);
    }
}
