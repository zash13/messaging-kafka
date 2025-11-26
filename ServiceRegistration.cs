using Messaging.Kafka.Config;
using Messaging.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Messaging.Kafka.Interface;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Messaging.Kafka

{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConsumerKafkaOptions>(configuration.GetSection("ConsumerKafkaOptions"));
            services.Configure<ProducerKafkaOptions>(configuration.GetSection("ProducerKafkaOptions"));
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
            services.AddSingleton<KafkaConsumer>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<KafkaConsumer>());
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IEnvelopeDataHelper, EnvelopeDataHelper>();
            services.AddSingleton<EnvelopeRouter>();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            services.AddSingleton(jsonOptions);
            return services;
        }
    }
}
