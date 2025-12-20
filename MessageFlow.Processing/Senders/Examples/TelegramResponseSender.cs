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
        Dictionary<string, string> metaData,
        HandlerResult handlerResult,
        CancellationToken cancellationToken
    )
    {
        /*
          #region mapping
          if (handlerResult.Data == null && handlerResult.UserMessage == null)
              return;
          if (!long.TryParse(metaData.GetValue("chat_id"), out var chatId))
          {
              Console.WriteLine("fuck");
              return;
          }
          foreach (var item in metaData)
              Console.WriteLine($"reitem {item.Key} , {item.Value}");
          Console.WriteLine("chatId");
  
          var response = TelegramTextResponse.Create("text", handlerResult.UserMessage, chatId);
          #endregion
  
          #region publish
          // and then publish your message in here
          var correlationId = "0";
  
          // iam not in mode to include them
          await _producer.ProduceAsync(
              topic: MessageContracts.Constants.MessageTopic.Response.Name,
              eventType: MessageContracts.Constants.MessageType.ResponsText.Type,
              channel: "telegram",
              payload: response,
              key: $"telegram-{chatId}",
              correlationId: correlationId,
              cancellationToken: cancellationToken
          );
          #endregion
         */
    }
}
