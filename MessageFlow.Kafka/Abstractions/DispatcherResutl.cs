namespace MessageFlow.Kafka.Abstractions
{
    public class DispatcherResutl
    {
        public bool ShouldCommit { get; }
        private DispatcherResutl(bool shouldCommit)
        {
            ShouldCommit = shouldCommit;
        }
        public static DispatcherResutl Commit() => new(true);
        public static DispatcherResutl NoCommit() => new(false);
    }
}
