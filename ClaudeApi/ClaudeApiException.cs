namespace ClaudeApi
{
    public partial class Client
    {
        public class ClaudeApiException : Exception
        {
            public ClaudeApiException(string message, Exception? innerException = null) : base(message, innerException) { }
        }
    }
}