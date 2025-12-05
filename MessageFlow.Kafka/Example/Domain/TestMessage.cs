using MessageFlow.Kafka.Abstractions;
namespace Example.Domain
{
    public class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IList<string> TestList { get; set; } = new List<string>();
        public DateTime Timestemp { get; set; } = DateTime.Now;
        public string Key
        {
            get { return $"{Id} + {Name}"; }
        }
    }
}
