// add this part to your program.cs
/*
     builder.Services.Configure<ProducerKafkaOptions>(
        builder.Configuration.GetSection("Kafka")
    );
*/
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
            services.AddScoped<ISerializer, SystemTextJsonSerializer>();
            services.AddScoped<IKafkaConsumer, KafkaConsumer>();
            services.AddScoped<IKafkaProducer, KafkaProducer>();
            KafkaConfiguration(services);
            return services;
        }

        public static void KafkaConfiguration(IServiceCollection services)
        {
            // your topics in here
            var topics = new List<string> { "topic1", "topics2" };

            services.AddSingleton(topics);
        }
    }
}
