public static class MetadataExtension
{
    public static string? GetValue(this Dictionary<string, string>? metadata, string key)
    {
        return metadata?.GetValueOrDefault(key);
    }

    public static T? GetValue<T>(this Dictionary<string, string>? metadata, string key,
        Func<string, T>? converter = null) where T : class
    {
        if (metadata == null || !metadata.TryGetValue(key, out var value))
            return default;

        if (converter != null)
            return converter(value);

        if (typeof(T) == typeof(int) && int.TryParse(value, out var intVal))
            return (T)(object)intVal;
        if (typeof(T) == typeof(bool) && bool.TryParse(value, out var boolVal))
            return (T)(object)boolVal;
        if (typeof(T) == typeof(decimal) && decimal.TryParse(value, out var decimalVal))
            return (T)(object)decimalVal;

        return value as T;
    }

    public static bool HasKey(this Dictionary<string, string>? metadata, string key)
    {
        return metadata?.ContainsKey(key) ?? false;
    }
}

