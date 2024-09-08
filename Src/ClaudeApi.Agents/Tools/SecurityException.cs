using System;

namespace ClaudeApi.Agents.Tools
{
    public class SecurityException : Exception
    {
        public SecurityException() : base() { }

        public SecurityException(string message) : base(message) { }

        public SecurityException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
