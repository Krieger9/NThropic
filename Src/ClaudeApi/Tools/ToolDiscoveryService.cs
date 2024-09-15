using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NJsonSchema;

namespace ClaudeApi.Tools
{
    public class ToolDiscoveryService(IServiceProvider serviceProvider)
    {
        public List<Tool> DiscoverTools(Assembly assembly)
        {
            var tools = new List<Tool>();

            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(ToolAttribute), false).Length > 0)
                .ToList();

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<ToolAttribute>();
                var parameters = method.GetParameters();

                var inputSchema = new JsonSchema
                {
                    Type = JsonObjectType.Object
                };

                foreach (var parameter in parameters)
                {
                    var property = new JsonSchemaProperty
                    {
                        Type = ConvertTypeToJsonObjectType(parameter.ParameterType)
                    };
                    inputSchema.Properties.Add(parameter.Name ?? string.Empty, property);
                }

                var toolType = method.DeclaringType;
                if (toolType != null && toolType.GetCustomAttribute<RequiresInitializationAttribute>() != null)
                {
                    var initializeMethod = toolType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
                    initializeMethod?.Invoke(null, [serviceProvider]);
                }

                tools.Add(new Tool
                {
                    Name = attribute?.Name ?? string.Empty,
                    Description = attribute?.Description ?? string.Empty,
                    InputSchema = inputSchema,
                    Method = method
                });
            }

            return tools;
        }

        private static JsonObjectType ConvertTypeToJsonObjectType(Type type)
        {
            if (type == typeof(string)) return JsonObjectType.String;
            if (type == typeof(int) || type == typeof(long)) return JsonObjectType.Integer;
            if (type == typeof(float) || type == typeof(double)) return JsonObjectType.Number;
            if (type == typeof(bool)) return JsonObjectType.Boolean;
            return JsonObjectType.Object;
        }
    }
}