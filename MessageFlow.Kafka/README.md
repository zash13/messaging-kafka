# MessageFlow.Kafka

A lightweight .NET wrapper for Kafka producers and consumers, providing envelope-based message handling, partitioning via keys, and easy integration into web APIs or console apps.

## about project  
This wrapper simplifies working with Kafka in .NET applications by abstracting producer and consumer logic. It uses an envelope structure for messages, supports custom handlers for event types, and integrates seamlessly with ASP.NET Core web APIs. 

## How to use the wrapper

### Configurations 
  1. change `Configuration/consumer.config.json` and `Configuration/producer.config json` :    
      - change `BootstrapServers` to address of your kafka server 
      + change `topics list` base on our desier topics 
      > ! do not change `EnableAutoCommit` 
  2. change `ServiceRegistration.cs`
      > you can have both consumer and producer or eather one , read my comment there 
## Setup and Use 
#### Setup in a Web API: 
  registers the Kafka services  with dependency injection.
  > add this to your program.cs
  ``` C#
var builder = WebApplication.CreateBuilder(args);
// Load Kafka configs (order matters: config first, then services)
builder.Configuration.AddKafkaEmbeddedConfigs();
builder.Services.AddKafkaMessaging(builder.Configuration);
  ```     

#### Creating a Message Model
> All messages must implement the `IQueueMessage` interface.
``` C# 
public class TestMessage : IQueueMessage
{
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
// whatever fields you need
// the key is used for kafka partitioning — change it as you wish
public string Key => $"{Id}_{Name}";  
}
  ``` 

  - [read about partitioning key ](https://www.confluent.io/learn/kafka-partition-key/#impact-of-partition-keys-on-kafka-performance)
  -  Input/Output: The model holds your event data. When producing, it's wrapped in an Envelope (see below). No direct output; it's serialized to JSON for Kafka .

#### Creating a Handler
Handlers process consumed messages. They implement `IEnvelopeHandler<TEvent>` and are decorated with `[EnvelopeHandler("EventType")]` to match the envelope's type.
> simply just add this to you handler class which inherited form `IEnvelopeHandler<YouModel>`
``` C#
[EnvelopeHandler("YourEventName")]
```
> Keep in mind that `YourEventName` and `envelope_type` passed to the producer must match; otherwise, the router cannot find your handler.  

> `YourModel` is the model you created in the previous section, and it inherits from `IQueueMessage`.

## Producing and Consuming Message
#### Producing 
  + Inject IKafkaProducer and use ProduceAsync.
  ``` C#
        var message = new TestMessage
        {
            Id = Random.Shared.Next(1, 9999),
            Name = $"test_{DateTime.Now:HHmmss}",
        };

        await _producer.ProduceAsync(
            topic: "topic1",
            eventType: "TestEvent",  // MUST MATCH HANDLER'S ENVELOPEHANDLER ATTRIBUTE
            eventMessage: message,
            correlationId: Guid.NewGuid().ToString()  // Optional for tracing
        );
```

#### Consuming Messages 
- Consumers run in the background after setup. Messages are deserialized, matched to handlers by EnvelopeType, and processed asynchronously.
## Sources & Credits
[csharp multithreading](https://www.tutorialspoint.com/csharp/csharp_multithreading.htm) 

[more about Task and threading](https://www.geeksforgeeks.org/c-sharp/types-of-threads-in-c-sharp/) 

[Kafka Examples](https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/Consumer/Program.cs) 
# What's next
- Implement order preservation: current logic isn’t respecting order, so I’m sticking to one partition and praying my messages stay in order 

- Monitoring threads: Implement diagnostics to track active threads, their status, and count (e.g., using ThreadPool metrics or custom logging).

- Monitoring the wrapper using Prometheus: Add instrumentation for metrics like message throughput, errors, and latency, exposed via a Prometheus endpoint.

- Change the structure: Refactor for better layout, such as modularizing configs, improving handler discovery, or updating inner logic for better error resilience and versioning.
