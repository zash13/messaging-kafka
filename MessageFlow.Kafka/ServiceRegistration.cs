using System.Text.Json;
using System.Text.Json.Serialization;
using MessageFlow.Kafka.Internals;
using MessageFlow.Kafka.Abstractions;
using MessageFlow.Kafka.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using MessageFlow.Processing.Senders;
using MessageFlow.Processing.Senders.Abstractions;
using MessageFlow.Processing.Common;
// order is matter 
// you can comment out either the consumer or the producer if you want.
namespace MessageFlow.Kafka
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            #region Common Dependencies
            services.Configure<ConsumerKafkaOptions>(configuration.GetSection("ConsumerKafkaOptions"));
            services.Configure<ProducerKafkaOptions>(configuration.GetSection("ProducerKafkaOptions"));
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            services.AddSingleton(jsonOptions);
            #endregion

            #region Register envelop handlers and create route rdictionary 
            var handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttribute<EnvelopHandlerAttribute>() != null).ToList();
            var routeDictionary = new Dictionary<string, Type>();
            foreach (var handler in handlers)
            {
                var attr = handler.GetCustomAttribute<EnvelopHandlerAttribute>()!;
                routeDictionary[attr.EnvelopType] = handler;
                // i dont know if this is corrent desision to register handlers in here or what 
                // for now i keep it like this 
                // but i dont know how this will effect threads and ...
                services.AddTransient(handler);
            }

            #region response Dependencies

            // register your senders over here 
            //services.AddScoped<IResponseSender, WebResponseSender>();
            //services.AddScoped<IResponseSender, TelegramResponseSender>();
            services.AddSingleton<IResponseSenderFactory, ResponseSenderFactory>();
            #endregion

            #endregion
            #region Consumer Dependencies
            services.AddSingleton<IEnvelopeDataHelper, EnvelopeDataHelper>();
            services.AddSingleton<IEnvelopeRouter>(sp => new EnvelopeRouter(sp, sp.GetRequiredService<IEnvelopeDataHelper>(), routeDictionary));
            services.AddSingleton<IMessagDispatcher>(sp => new MessagDispatcher(sp.GetRequiredService<IEnvelopeRouter>(), sp.GetRequiredService<IResponseSenderFactory>(), maxConcurrency: 100));
            services.AddSingleton<KafkaConsumer>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<KafkaConsumer>());

            #endregion
            #region Producer Dependencies
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            #endregion

            return services;
        }
    }
}

