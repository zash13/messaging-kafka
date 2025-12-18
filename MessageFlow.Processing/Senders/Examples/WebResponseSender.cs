
using MessageFlow.Processing.Common;
public sealed class WebResponseSender : IResponseSender
{
    private readonly IKafkaProducer _producer;

    public WebResponseSender(IKafkaProducer producer)
    {
        _producer = producer;
    }

    public string ChannelType => "web";

    public async Task SendAsync(
        Dictionary<string, string> metaData,
        HandlerResult handlerResult,
        CancellationToken cancellationToken)
    {

        #region mapping
        object response = new object();
        // your maping happens here 
        /*
          WebGeneralResponse response;
          if (handlerResult.IsSuccess)
          {
              response = WebResponse<object>.Success(
                  handlerResult.Data,
                  Message: handlerResult.UserMessage ?? "Success");
          }
          else
          {
              response = WebResponse<object>.Failure(
                  handlerResult.UserMessage ?? "Request failed");
          }
         */
        #endregion

        #region publish 
        // and then publish your message in here 
        object payload = new();
        object context = new();
        long chatId = 1;
        var correlationId = "0";

        await _producer.ProduceAsync(
            topic: "web.responses",
            eventType: "web.response",
            channel: "telegram",
            payload: payload,
            key: $"telegram-{chatId}",
            correlationId: correlationId,
            cancellationToken: cancellationToken
        );

        #endregion
    }
}
