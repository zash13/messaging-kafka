using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MessageFlow.Kafka.Configuration
{

    public static class EmbeddedConfiguration
    {
        public static IConfigurationBuilder AddKafkaEmbeddedConfigs(this IConfigurationBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            /*
            // list for debugging
            foreach (var n in _assembly.GetManifestResourceNames())
                Console.WriteLine(n);
             */

            AddEmbeddedJsonConfig(builder, assembly, "Messaging.Kafka.Configuration.consumer.config.json");
            AddEmbeddedJsonConfig(builder, assembly, "Messaging.Kafka.Configuration.producer.config.json");

            return builder;
        }

        private static void AddEmbeddedJsonConfig(IConfigurationBuilder builder, Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
            var embeddedConfig = new ConfigurationBuilder()
                                    .AddJsonStream(stream)
                                    .Build();

            builder.AddConfiguration(embeddedConfig);

        }
    }
}
