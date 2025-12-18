// this envelop class is now too complecated , i dont like it and it need to change , once again , become simpler then the object that hiddedn beginde payload can be anything 
using System.Text.Json.Serialization;
namespace MessageFlow.Processing.Common
{
    public abstract class BaseEnvelope
    {
        // I’ve been thinking about how to change this to a more reliable method,
        // instead of relying on a string (EventType). However, when you consider
        // cross-language scenarios—like a Python sender and a C# receiver—
        // along with other factors, it’s difficult for me to replace this part with anything else.
        [JsonPropertyName("envelope_id")]
        public Guid EnvelopeId { get; set; } = Guid.NewGuid();

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = default!; // login ,shopcard ....

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = default!;//telegram , web , other interfaces or users 

        // i dont know where , but version may be usefull in future 
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("correlation_id")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;


    }
    public class Payload
    {

        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
        // this is new alternative instend of object data 
        [JsonPropertyName("body")]
        public object? Body { get; set; }
    }
    public class Envelope : BaseEnvelope
    {
        // don’t  tie your envelope to the payload 
        // If your meta is just for logging, fine — but don’t drag it into real logic.
        // Seriously, don’t use this shit. Put the damn meta inside the payload where it belongs.
        // blive me , it has cost 
        [JsonPropertyName("payload")]
        public Payload Payload { get; set; }

        // this is just here for id , nothing else , you should not create envelope by hand,  actualy , you should never see it , just use publisher methods 
        public static Envelope Create(
            string eventType,
            string channel,
            Payload? payload,
            int? version = 1,
            string? correlationId = null
        )
        {
            return new Envelope
            {
                EventType = eventType,
                Channel = channel,
                Payload = payload ?? new Payload(),
                Version = version ?? 1,
                CorrelationId = correlationId,
            };
        }
    }

}
