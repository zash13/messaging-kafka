using System.Text.Json;
using System.Text.Json.Serialization;
using MessageFlow.Kafka.Internals;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Kafka.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
// order is matter 
namespace MessageFlow.Kafka
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConsumerKafkaOptions>(configuration.GetSection("ConsumerKafkaOptions"));
            services.Configure<ProducerKafkaOptions>(configuration.GetSection("ProducerKafkaOptions"));
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
            services.AddSingleton<IEnvelopeDataHelper, EnvelopeDataHelper>();
            services.AddSingleton<EnvelopeRouter>();
            services.AddSingleton<IMessageDispatcher>(sp => new MessagDispatcher(sp.GetRequiredService<EnvelopeRouter>(), maxConcurrency: 100));
            services.AddSingleton<KafkaConsumer>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<KafkaConsumer>());
            services.AddSingleton<IKafkaProducer, KafkaProducer>();

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
