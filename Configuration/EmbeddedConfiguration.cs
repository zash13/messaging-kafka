using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MessageFlow.Kafka.Configuration
{

    public static class EmbeddedConfiguration
    {
        public static IConfigurationBuilder AddKafkaEmbeddedConfigs(this IConfigurationBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            AddEmbeddedJsonConfig(builder, assembly, "Messaging.Kafka.Configuration.consumer.config.json");
            AddEmbeddedJsonConfig(builder, assembly, "Messaging.Kafka.Configuration.producer.config.json");

            return builder;
        }

        private static void AddEmbeddedJsonConfig(IConfigurationBuilder builder, Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();
                var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
                builder.AddJsonStream(memoryStream);
            }
        }
    }
}
