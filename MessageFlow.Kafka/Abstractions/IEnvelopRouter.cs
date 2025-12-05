namespace MessageFlow.Kafka.Abstractions
{

    public interface IEnvelopeRouter
    {
        Task RouteAsync(Envelope envelope);
    }
}
