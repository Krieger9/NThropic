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

        // Validate that the tool has a name, description, input schema, and method
        public bool IsValid()
        {
            return Name != null && Description != null && InputSchema != null && Method != null;
        }
    }
}