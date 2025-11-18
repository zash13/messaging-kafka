using System.Collections.Generic;
using Messaging.Kafka.Config;
using Messaging.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messaging.Kafka
{
    public static class ServiceRegistration
    {
        public static IServiceCollection BrokerService(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<ProducerKafkaOptions>(configuration.GetSection("KafkaProducer"));
            services.Configure<ConsumerKafkaOptions>(configuration.GetSection("KafkaConsumer"));
            services.Configure<ConsumerKafkaOptions>(options =>
                options.Topics =
                    configuration.GetSection("KafkaConsumer:Topics").Get<List<string>>() ?? new()
                    {
                        "topic1",
                        "topic2",
                    }
            );
            services.AddScoped<ISerializer, SystemTextJsonSerializer>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
            services.AddHostedService<KafkaConsumer>();
            KafkaConfiguration(services);
            return services;
        }

        public static void KafkaConfiguration(IServiceCollection services) { }
    }
}
