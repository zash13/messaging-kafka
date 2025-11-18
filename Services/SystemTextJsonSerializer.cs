// simple Serialize , i ask GPT for it
using System;
using System.Text.Json;

namespace Messaging.Kafka.Services
{
    public class SystemTextJsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _opts;

        public SystemTextJsonSerializer()
        {
            _opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _opts);
        }

        public object? Deserialize(string json, Type targetType)
        {
            return JsonSerializer.Deserialize(json, targetType, _opts);
        }

        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _opts);
        }
    }
}
