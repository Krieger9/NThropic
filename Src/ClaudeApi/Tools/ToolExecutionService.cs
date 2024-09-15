using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClaudeApi.Messages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace ClaudeApi.Tools
{
    public class ToolExecutionService
    {
#pragma warning disable IDE0290 // Use primary constructor
        private readonly Dictionary<string, Tool> _tools;
        private readonly Dictionary<string, object> _toolInstances = [];
        private readonly IServiceProvider _serviceProvider;

        // ToolExecutionService.cs
        public ToolExecutionService(List<Tool> tools, IServiceProvider serviceProvider)
        {
            _tools = ValidateAndRegisterTools(tools);
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private static Dictionary<string, Tool> ValidateAndRegisterTools(List<Tool> tools)
        {
            var toolDictionary = new Dictionary<string, Tool>();
            foreach (var tool in tools)
            {
                if (string.IsNullOrWhiteSpace(tool.Name))
                {
                    throw new ArgumentException("Tool name cannot be null or empty.");
                }

                if (tool.Method == null)
                {
                    throw new ArgumentException($"Method for tool '{tool.Name}' cannot be null.");
                }

                if (toolDictionary.ContainsKey(tool.Name))
                {
                    throw new ArgumentException($"Duplicate tool name '{tool.Name}' found.");
                }

                ValidateToolSchema(tool);

                toolDictionary[tool.Name] = tool;
            }
            return toolDictionary;
        }

        private static void ValidateToolSchema(Tool tool)
        {
            if (tool.InputSchema == null)
            {
                throw new ArgumentException($"Input schema for tool '{tool.Name}' cannot be null.");
            }

            if (tool.Method == null)
            {
                throw new ArgumentException($"Method for tool '{tool.Name}' cannot be null.");
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

        public async Task<string> ExecuteToolAsync(string toolName, JObject input, Client client, List<Message> messages)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                throw new ArgumentException("Tool name cannot be null or empty.");
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Tool input cannot be null.");
            }

            if (!_tools.TryGetValue(toolName, out var tool))
            {
                throw new ArgumentException($"Tool '{toolName}' not found.");
            }

            if (tool.Method == null)
            {
                throw new ArgumentException($"Method for tool '{toolName}' not found.");
            }

            var toolInstance = GetOrCreateToolInstance(tool);

            var parameters = tool.Method.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.Name == null)
                {
                    throw new InvalidOperationException($"Parameter name is null for tool '{toolName}' at index {i}.");
                }

                if (param.ParameterType == null)
                {
                    throw new InvalidOperationException($"Parameter type is null for parameter '{param.Name}' in tool '{toolName}'.");
                }

                if (!input.TryGetValue(param.Name, out var value))
                {
                    if (ParameterDefaultValueHelper.TryGetDefaultValue(param, out var defaultValue))
                    {
                        args[i] = defaultValue!;
                        continue;
                    }
                    throw new ArgumentException($"Missing required parameter '{param.Name}' for tool '{toolName}'.");
                }

                try
                {
                    args[i] = ConvertValue(value, param.ParameterType, param.Name, toolName) ??
                        (ParameterDefaultValueHelper.TryGetDefaultValue(param, out var defaultValue)
                            ? defaultValue!
                            : throw new ArgumentException($"Null value not allowed for non-optional parameter '{param.Name}' in tool '{toolName}'."));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to convert input value for parameter '{param.Name}' to type '{param.ParameterType.Name}' in tool '{toolName}': {ex.Message}", ex);
                }
            }

            try
            {
                var result = tool.Method.Invoke(toolInstance, args);

                // Check if the result is a ToolInvokeResult
                if (result is ToolInvokeResult toolInvokeResult)
                {
                    // Call SystemCommand if it is not null
                    toolInvokeResult.SystemCommand?.Invoke(client, messages);

                    // Use the Value property as the return value
                    result = toolInvokeResult.Value;
                }

                var resultString = await ProcessToolResult(result, toolName);
                return resultString;
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidOperationException($"Error executing tool '{toolName}': {ex.InnerException?.Message ?? ex.Message}", ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error executing tool '{toolName}': {ex.Message}", ex);
            }
        }

        private object GetOrCreateToolInstance(Tool tool)
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

        private static object? ConvertValue(JToken value, Type targetType, string paramName, string toolName)
        {
            if (value.Type == JTokenType.Null)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString(), ignoreCase: true);
            }

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value.ToString(), out var dateTime))
                {
                    return dateTime;
                }
                throw new ArgumentException($"Invalid date format for parameter '{paramName}' in tool '{toolName}'.");
            }

            try
            {
                return value.ToObject(targetType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert value '{value}' to type '{targetType.Name}' for parameter '{paramName}' in tool '{toolName}': {ex.Message}", ex);
            }
        }

        private static async Task<string> ProcessToolResult(object? result, string toolName)
        {
            if (result == null)
            {
                return $"Tool '{toolName}' executed successfully with null result.";
            }
            if (result is Task<string> taskStringResult)
            {
                return await taskStringResult;
            }
            else if (result is Task<object> taskObjectResult)
            {
                var objResult = await taskObjectResult;
                return objResult?.ToString() ?? "Tool executed successfully with null result.";
            }
            else if (result is Task task)
            {
                await task;
                return "Tool executed successfully (void return type).";
            }
            else if (result == null)
            {
                return "Tool executed successfully with null result.";
            }
            else
            {
                return result.ToString()!;
            }
        }
    }
}