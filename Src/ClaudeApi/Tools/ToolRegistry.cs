using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Tools
{
    public class ToolRegistry : IToolRegistry
    {
        private readonly Dictionary<string, Tool> _tools = new();
        private readonly Dictionary<string, object> _toolInstances = new();
        private readonly IServiceProvider _serviceProvider;

        public IEnumerable<Tool> Tools => _tools.Values;

        public ToolRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Dictionary<string, Tool> ValidateAndRegisterTools(List<Tool> tools)
        {
            var toolDictionary = new Dictionary<string, Tool>();
            foreach (var tool in tools)
            {
                AddTool(tool);
            }
            return toolDictionary;
        }

        public void AddTool(Tool tool)
        {
            ValidateTool(tool);
            _tools[tool.Name!] = tool; // Overwrite if duplicate
        }

        public void AddTools(IEnumerable<Tool> tools)
        {
            foreach (var tool in tools)
            {
                AddTool(tool);
            }
        }

        public void RemoveTool(string toolName)
        {
            if (!_tools.Remove(toolName))
            {
                throw new ArgumentException($"Tool '{toolName}' not found.");
            }
            _toolInstances.Remove(toolName);
        }

        private static void ValidateTool(Tool tool)
        {
            if (string.IsNullOrWhiteSpace(tool.Name))
            {
                throw new ArgumentException("Tool name cannot be null or empty.");
            }

            if (tool.Method == null)
            {
                throw new ArgumentException($"Method for tool '{tool.Name}' cannot be null.");
            }

            if (tool.InputSchema == null)
            {
                throw new ArgumentException($"Input schema for tool '{tool.Name}' cannot be null.");
            }

            var parameters = tool.Method.GetParameters();
            foreach (var param in parameters)
            {
                if (param.Name == null)
                {
                    throw new ArgumentException($"Parameter name is null in tool '{tool.Name}'.");
                }

                if (!tool.InputSchema.Properties.TryGetValue(param.Name, out var schemaProperty))
                {
                    throw new ArgumentException($"Input schema for tool '{tool.Name}' is missing property '{param.Name}'.");
                }

                if (!IsSchemaTypeCompatible(schemaProperty, param.ParameterType))
                {
                    throw new ArgumentException($"Schema type '{schemaProperty.Type}' is not compatible with parameter type '{param.ParameterType.Name}' for parameter '{param.Name}' in tool '{tool.Name}'.");
                }
            }
        }

        private static bool IsSchemaTypeCompatible(JsonSchemaProperty schemaProperty, Type parameterType)
        {
            return schemaProperty.Type switch
            {
                JsonObjectType.String => parameterType == typeof(string),
                JsonObjectType.Number => parameterType == typeof(double) || parameterType == typeof(float),
                JsonObjectType.Integer => parameterType == typeof(int) || parameterType == typeof(long),
                JsonObjectType.Boolean => parameterType == typeof(bool),
                JsonObjectType.Object => parameterType.IsClass,
                JsonObjectType.Array => parameterType.IsArray || (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(List<>)),
                _ => false,
            };
        }

        public object GetOrCreateToolInstance(Tool tool)
        {
            if (tool.Name == null)
            {
                throw new ArgumentNullException(nameof(tool), "Tool name cannot be null.");
            }

            if (tool.Method?.DeclaringType == null)
            {
                throw new InvalidOperationException("DeclaringType of the tool's method cannot be null.");
            }

            if (!_toolInstances.TryGetValue(tool.Name, out var instance))
            {
                instance = _serviceProvider.GetRequiredService(tool.Method.DeclaringType);
                _toolInstances[tool.Name] = instance;
            }
            return instance;
        }

        public bool TryGetTool(string toolName, out Tool? tool)
        {
            return _tools.TryGetValue(toolName, out tool);
        }
    }
}
