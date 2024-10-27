using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using Newtonsoft.Json;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents.Converters
{
    public class GenericConverterAgent : Agent, IConverterAgent
    {
        private readonly ClaudeClient _client;

        public GenericConverterAgent(ClaudeClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public override async Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments)
        {
            var desiredType = arguments["desiredType"] as Type;
            if (desiredType == null)
            {
                throw new ArgumentException("desiredType argument is required and must be a Type.", nameof(arguments));
            }

            var objectResult = await ConvertToAsync(input, desiredType);
            return objectResult?.ToString() ?? string.Empty;
        }

        public async Task<object?> ConvertToAsync(string input, Type desiredType)
        {
            // Check if the desired type is an enum
            if (desiredType.IsEnum)
            {
                try
                {
                    return Enum.Parse(desiredType, input, ignoreCase: true);
                }
                catch (Exception)
                {
                    // Ignore exceptions and proceed to use the prompt
                }
            }

            // Check if input is JSON and matches the desired type
            try
            {
                var schema = JsonSchema.FromType(desiredType);
                var errors = schema.Validate(input);
                if (!errors.Any())
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
                            { "input", input },
                            { "schema", JsonSchema.FromType(desiredType).ToJson() }
                        }
            };

            var history = new List<Message>();
            var systemMessage = new List<ContentBlock>();
            var responses = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage);

            return JsonConvert.DeserializeObject(await responses.ToSingleStringAsync(), desiredType);
        }
    }
}
