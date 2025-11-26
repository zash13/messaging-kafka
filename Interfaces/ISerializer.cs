namespace Messaging.Kafka.Interface
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object? Deserialize(string json, Type targetType);
        T? Deserialize<T>(string json);
    }
}
