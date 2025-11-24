using Confluent.Kafka;
using Messaging.Kafka.Config;
using Microsoft.Extensions.Hosting;

namespace Messaging.Kafka.Services
{
    public interface IKafkaConsumer : IHostedService, IDisposable
    {

        void ConsumeSingleMessage(string topic);

        void ConsumeMessages(string topic);

        void Commit(ConsumeResult<string, string> consumeResult);

    }
}
