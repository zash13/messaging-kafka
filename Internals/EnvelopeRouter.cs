using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MessageFlow.Kafka.Abstractions;


namespace MessageFlow.Kafka.Internals
{

    public class EnvelopeRouter
    {
        private readonly IServiceProvider _provider;
        private readonly IEnvelopeDataHelper _dataHelper;

        public EnvelopeRouter(IServiceProvider provider, IEnvelopeDataHelper dataHelper)
        {
            _provider = provider;
            _dataHelper = dataHelper;
        }
        public async Task RouteAsync(Envelope envelope)
        {
            Console.WriteLine("=== ROUTING DEBUG START ===");
            Console.WriteLine($"Envelope is null? {envelope == null}");

            if (envelope == null)
            {
                Console.WriteLine("fuck – envelope is null");
                return;
            }

            Console.WriteLine($"EnvelopeType from message: '{envelope.EnvelopeType}'");
            Console.WriteLine($"Data is null? {envelope.Data == null}");

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException) { return Type.EmptyTypes; }
                })
                .Where(t => t.GetCustomAttribute<EnvelopHandlerAttribute>() != null)
                .ToList();

            Console.WriteLine($"Found {allTypes.Count} types with EnvelopHandler attribute:");

            foreach (var t in allTypes)
            {
                var attr = t.GetCustomAttribute<EnvelopHandlerAttribute>();
                Console.WriteLine($"  → {t.Name} handles '{attr?.EnvelopType}'");
            }

            var handlerType = allTypes.FirstOrDefault(t =>
                t.GetCustomAttribute<EnvelopHandlerAttribute>()?.EnvelopType == envelope.EnvelopeType);

            if (handlerType == null)
            {
                Console.WriteLine($"No handler found for EnvelopeType = '{envelope.EnvelopeType}'");
                Console.WriteLine("Available types above ↑↑↑");
                Console.WriteLine("=== ROUTING DEBUG END ===");
                return; // don't throw, just debug
            }

            // New – perfect for your use-case:
            var handler = ActivatorUtilities.CreateInstance(_provider, handlerType);

            // 3. Rest stays exactly the same
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeHandler<>));

            var payloadType = interfaceType.GetGenericArguments()[0];

            var payload = _dataHelper.CreatePayload(envelope.Data, payloadType);
            if (payload == null)
            {
                Console.WriteLine("CreatePayload failed – check JSON ↔ model mapping");
                return;
            }

            var method = interfaceType.GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, new object[] { payload, CancellationToken.None })!;

            Console.WriteLine("ROUTING SUCCESS");
        }

        static string TypeName(Type t) => t.FullName ?? t.Name;
        /*
        public async Task RouteAsync(Envelope envelope)
        {
            var handlerType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t =>
                    t.GetCustomAttribute<EnvelopHandlerAttribute>()?.EnvelopType == envelope.EnvelopeType);

            if (handlerType == null)
                throw new Exception($"No handler found for event type: {envelope.EnvelopeType}");

            var handler = _provider.GetService(handlerType);
            if (handler == null)
                throw new Exception($"Handler type {handlerType.Name} not registered");

            // here i’m trying to cast object? data to your Data model type, which inherits from IEnvelopeData.
            // it’s important to create your model using this interface.
            // additionally, your handler should inherit from IEnvelopeHandler<YourDataModel>
            // where YourDataModel implements IEnvelopeData.
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeHandler<>));
            var payloadType = interfaceType.GetGenericArguments()[0];

            // null .Data may break things here!
            var payload = _dataHelper.CreatePayload(envelope.Data, payloadType);

            var method = interfaceType.GetMethod("HandleAsync");
            await (Task)method.Invoke(handler, new[] { payload })!;
        }
         */
    }
}
