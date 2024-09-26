namespace ClaudeApi
{
    public partial class ClaudeClient
    {
        public class ClaudeApiException : Exception
        {
            public ClaudeApiException(string message, Exception? innerException = null) : base(message, innerException) { }
        }
    }
}