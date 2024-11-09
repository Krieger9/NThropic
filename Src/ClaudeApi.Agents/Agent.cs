using System.Reflection;

namespace ClaudeApi.Agents
{
    public class Agent : IAgent
    {
        public virtual string SystemPrompt { get; set; } = "You are a helpful assistant";

        public virtual async Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments)
        {
            // Look for a method with the AgentExecuter attribute
            var method = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => m.GetCustomAttribute<AgentExecutorAttribute>() != null);

            if (method != null)
            {
                var parameters = method.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
                {
                    var args = new object?[parameters.Length];
                    args[0] = input;

                    for (int i = 1; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        if (arguments.TryGetValue(param.Name!, out var value))
                        {
                            if (value == null && param.ParameterType.IsValueType && Nullable.GetUnderlyingType(param.ParameterType) == null)
                            {
                                throw new ArgumentException($"Argument '{param.Name}' cannot be null.");
                            }
                            args[i] = value;
                        }
                        else if (param.HasDefaultValue)
                        {
                            args[i] = param.DefaultValue;
                        }
                        else
                        {
                            throw new ArgumentException($"Argument '{param.Name}' is required.");
                        }
                    }

                    var result = method.Invoke(this, args);
                    if (result is Task<string> taskResult)
                    {
                        return await taskResult;
                    }
                }
            }

            // Default behavior if no method is found
            return await Task.FromResult("");
        }
    }
}
