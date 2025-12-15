// this class is my solution to abstract handlers from kafka.
// since handlers should only handle messages, i want them to avoid being involved in the messaging solution itself.
// the core problem is how to send the response. handlers should not be responsible for that.
// the presentation layer is only a mapping from semi-raw data (retrieved from the database) to a contracted json format.
// therefore, the dispatcher is the only component that knows enough to handle this properly.
// i still have doubts about my threading implementation, but i plan to call the producer there
// even though it might look wrong.
namespace MessageFlow.Handlers.Abstractions
{
    // soon or later , there will be an enum to seprate different status of handlers , i feel it 
    public class HandlerResult
    {
        // try not to create an instance from this class!!! :>
        private HandlerResult() { }

        #region Propertyes 

        public bool IsSuccess { get; private set; }
        public string? ServerMessage { get; private set; }
        public object? Data { get; private set; }
        public DateTimeOffset? Timestemp { get; private set; }

        public string? UserMessage { get; set; }
        // i may add other fields to this class in the future,
        // such as execution time, retry counter, commit flag,
        // error message, or an enum to specify the state of a message
        // (for multi-stage messaging).
        // additional fields that i don’t yet know may also be added later
        //
        //
        public bool ShouldCommit { get; private set; }

        #endregion


        #region factory methods 

        // this methods need summery , i know , i will do it in future 
        public static HandlerResult Success(string? userMessage, object? data = null, DateTimeOffset? timestemp = null) => new HandlerResult
        {
            IsSuccess = true,
            ServerMessage = "Success",
            UserMessage = userMessage,
            Data = data,
            Timestemp = timestemp,
            ShouldCommit = true,
        };
        public static HandlerResult GeneralUserFailure(string serverMessage, string? userMessage = null, object? data = null, DateTimeOffset? timestemp = null) => new HandlerResult
        {
            IsSuccess = false,
            ServerMessage = serverMessage,
            UserMessage = userMessage,
            Data = data,
            Timestemp = timestemp,
            ShouldCommit = true,

        };
        public static HandlerResult ValidationError(string? serverMessage = null, object? validationErrors = null, string? userMessage = "The request contains invalid data.") => new HandlerResult
        {
            IsSuccess = false,
            UserMessage = userMessage,
            ServerMessage = serverMessage,
            Data = validationErrors,
            ShouldCommit = true,

        };
        public static HandlerResult ServerFailuer(string serverMessage, string? userMessage = null, DateTimeOffset? timestemp = null) => new HandlerResult
        {
            IsSuccess = false,
            ServerMessage = serverMessage,
            UserMessage = userMessage,
            Timestemp = timestemp,
            ShouldCommit = true,
        };
        // retrying immidetly is my only option for now , i may improve this part in future 
        public static HandlerResult HandlerFailuer(string serverMessage, string? userMessage = null, DateTimeOffset? timestemp = null) => new HandlerResult
        {
            IsSuccess = false,
            ServerMessage = serverMessage,
            UserMessage = userMessage,
            Timestemp = timestemp,
            ShouldCommit = false,
        };

        #endregion
    }
}
