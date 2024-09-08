using System;

namespace ClaudeApi.Tools
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ToolAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public ToolAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}