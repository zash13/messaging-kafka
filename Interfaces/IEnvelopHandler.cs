namespace Messaging.Kafka.Interface
{

    public interface IEnvelopeHandler<TData> where TData : IEnvelopeData
    {
        Task HandelAsync(TData data);

    }
}
