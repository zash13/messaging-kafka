
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
        Envelope envelope,
        HandlerResult handlerResult,
        CancellationToken ct)
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
        await _producer.ProduceAsync(
            topic: "web.responses",
            envelopType: "web.response",
            message: response,
            key: envelope.AggregateId,
            correlationId: envelope.CorrelationId
        );

        #endregion
    }
}
