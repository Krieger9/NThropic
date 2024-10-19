using System;

namespace ClaudeApi.Agents
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AgentExecutorAttribute : Attribute
    {
    }
}
