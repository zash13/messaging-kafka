using System.Reflection;
using Microsoft.Extensions.Configuration;

public static class EmbeddedConfiguration
{
    public static IConfigurationBuilder AddKafkaEmbeddedConfigs(this IConfigurationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var consumerStream = assembly.GetManifestResourceStream("Messaging.Kafka.consumer.config.json");
        if (consumerStream != null)
        {
            builder.AddJsonStream(consumerStream);
        }

        using var producerStream = assembly.GetManifestResourceStream("Messaging.Kafka.producer.config.json");
        if (producerStream != null)
        {
            builder.AddJsonStream(producerStream);
        }

        return builder;
    }
}
