using NJsonSchema;
using System.ComponentModel;
using System.Reflection;

namespace ClaudeApi.Tools
{
    public class ToolDiscoveryService
    {
        private readonly IServiceProvider _serviceProvider;

        public ToolDiscoveryService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public List<Tool> DiscoverTools(Assembly assembly)
        {
            var tools = new List<Tool>();

            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(ToolAttribute), false).Length > 0)
                .ToList();

            foreach (var method in methods)
            {
                var tool = CreateToolFromMethod(method);
                if (tool != null)
                {
                    tools.Add(tool);
                }
            }

            return tools;
        }

        public List<Tool> DiscoverTools(Type type)
        {
            var tools = new List<Tool>();

            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(ToolAttribute), false).Length > 0)
                .ToList();

            foreach (var method in methods)
            {
                var tool = CreateToolFromMethod(method);
                if (tool != null)
                {
                    tools.Add(tool);
                }
            }

            return tools;
        }

        public Tool? DiscoverTool(Type type, string methodName)
        {
            var method = type.GetMethod(methodName);
            if (method == null || method.GetCustomAttribute<ToolAttribute>() == null)
            {
                return null;
            }

            return CreateToolFromMethod(method);
        }

        private Tool? CreateToolFromMethod(MethodInfo method)
        {
            var toolAttribute = method.GetCustomAttribute<ToolAttribute>();
            var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
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
                initializeMethod?.Invoke(null, new object[] { _serviceProvider });
            }

            return new Tool
            {
                Name = toolAttribute?.Name ?? string.Empty,
                Description = descriptionAttribute?.Description ?? string.Empty,
                InputSchema = inputSchema,
                Method = method
            };
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