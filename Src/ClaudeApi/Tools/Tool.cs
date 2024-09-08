using NJsonSchema;
using System.Reflection;

namespace ClaudeApi.Tools
{
    public class Tool
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public JsonSchema? InputSchema { get; set; }
        public MethodInfo? Method { get; set; }
    }
}