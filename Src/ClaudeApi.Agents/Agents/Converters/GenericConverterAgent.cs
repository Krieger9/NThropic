using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using Newtonsoft.Json;
using NJsonSchema;

namespace ClaudeApi.Agents.Agents.Converters
{
    public class GenericConverterAgent : Agent, IConverterAgent
    {
        private readonly IClient _client;

        public GenericConverterAgent(IClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public override async Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments)
        {
            var desiredType = arguments["desiredType"] as Type ?? throw new ArgumentException("desiredType argument is required and must be a Type.", nameof(arguments));
            var objectResult = await ConvertToAsync(input, desiredType);
            return objectResult?.ToString() ?? string.Empty;
        }

        public async Task<object?> ConvertToAsync(string input, Type desiredType)
        {
            // Check if the desired type is an enum
            if (Enum.TryParse(desiredType, input, ignoreCase: true, out var enumResult))
            {
                return enumResult;
            }
            // Check if input is JSON and matches the desired type
            try
            {
                var schema = JsonSchema.FromType(desiredType);
                var errors = schema.Validate(input);
                if (errors.Count == 0)
                {
                    var result = JsonConvert.DeserializeObject(input, desiredType);
                    if (result != null)
                    {
                        return await Task.FromResult(result);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore exceptions and proceed to use the prompt
            }

            // If input is not valid JSON or doesn't match the desired type, use the prompt
            var prompt = new Prompt("GenericConverter")
            {
                Arguments = new Dictionary<string, object>
                        {
                            { "context", input },
                            { "json_schema", JsonSchema.FromType(desiredType).ToJson() }
                        }
            };

            var history = new List<Message>();
            var systemMessage = new List<ContentBlock>();
            var (response, _) = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage);

            if (desiredType.IsEnum)
            {
                if (Enum.TryParse(desiredType, response, ignoreCase: true, out var enumResult2))
                {
                    return enumResult2;
                }
                throw new InvalidCastException($"Could not convert {response} to {desiredType.Name}");
            }
            else
            {
                return JsonConvert.DeserializeObject(response, desiredType);
            }
        }
    }
}
