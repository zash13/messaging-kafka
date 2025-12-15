
using MessageFlow.Processing.Common;
public sealed class TelegramResponseSender : IResponseSender
{
    private readonly IKafkaProducer _producer;

    public TelegramResponseSender(IKafkaProducer producer)
    {
        _producer = producer;
    }

    public string ChannelType => "telegram";

    public async Task SendAsync(
        Envelope envelope,
        HandlerResult handlerResult,
        CancellationToken ct)
    {
        #region mapping
        object response = new object();
        // your maping happens here 
        /*
        if (handlerResult.Data == null && handlerResult.UserMessage == null)
            return;

        var response = TelegramResponse<object>.Create(
            type: "text",
            data: handlerResult.Data,
            meta: TelegramMeta.Create(
                chatId: long.Parse(envelope.AggregateId),
                markdown: true
            )
        );

         */
        #endregion

        #region publish 
        // and then publish your message in here 
        await _producer.ProduceAsync(
            topic: "telegram.responses",
            envelopType: "telegram.response",
            message: response,
            key: envelope.AggregateId,
            correlationId: envelope.CorrelationId
        );

        #endregion
    }
}
