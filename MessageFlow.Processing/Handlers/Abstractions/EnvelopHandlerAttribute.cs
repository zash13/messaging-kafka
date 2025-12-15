[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EnvelopHandlerAttribute : Attribute
{
    // target type that you want to write your handler on 
    public string EnvelopType { get; }
    // good old java getter 
    public EnvelopHandlerAttribute(string envelopType)
    {
        EnvelopType = envelopType;
    }
}
