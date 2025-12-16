
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
        CancellationToken cancellationToken)
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
        object payload = new();
        object context = new();
        long chatId = 1;
        var correlationId = "0";

        await _producer.ProduceAsync(
            topic: "telegram.responses",
            eventType: "telegram.response",
            channel: "telegram",
            payload: payload,
            key: $"telegram-{chatId}",
            correlationId: correlationId,
            cancellationToken: cancellationToken
        );
    }

    #endregion
}
