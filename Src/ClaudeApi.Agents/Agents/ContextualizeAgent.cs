using ClaudeApi.Agents.Agents.Configs;
using ClaudeApi.Agents.ContextCore;
using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Prompts;
using Microsoft.Extensions.Options;
using NJsonSchema;
using Scriban;

namespace ClaudeApi.Agents.Agents
{
    public class ContextualizeAgent : Agent, IContextualizeAgent
    {
        private readonly ContextualizeAgentConfig _config;
        private readonly IClient _client;

        public ContextualizeAgent(IClient client, IOptions<ContextualizeAgentConfig> config)
        {
            _config = config.Value;
            _client = client;
        }

        public async Task<IContext?> Contextualize(IEnumerable<string> information, IContext? currentContext = null)
        {
            if (information == null)
            {
                return currentContext ?? Context.Empty;
            }
            return await ContextualizeInternal(information, currentContext);
        }

        public async Task<IContext?> ContextualizeInternal(IEnumerable<string> information, IContext? context)
        {
            var promptName = _config.BaseContextualizePromptFile;
            if (string.IsNullOrWhiteSpace(promptName))
            {
                throw new ArgumentException("BaseContextualizePromptFile is not set in the configuration.");
            }
            var backdrop = context == null ? "" : await GetBackdrop(context);
            Prompt prompt = new(promptName)
            {
                Arguments = new Dictionary<string, object> { 
                    ["information"] = information, 
                    ["backdrop"] = backdrop,
                    ["context_schema"] = JsonSchema.FromType<Context>().ToJson()
                }
            };
            string? modelName = _config.Model;
            string response;
            if (string.IsNullOrWhiteSpace(modelName))
            {
                (response, _) = await _client.ProcessContinuousConversationAsync(prompt, []);
            }
            else
            {
                (response, _) = await _client.ProcessContinuousConversationAsync(prompt, [], model: modelName);
            }
            // parse the response json to get the Context object.
            try
            {
                var created_context = Context.FromJson(response);

                return created_context;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing the response from the contextualize prompt: {ex.Message}");
            }
        }

        public Task<string> GetBackdrop(IContext context)
        {
            var script_file = _config.ContextAsBackdropPromptFile;
            if (string.IsNullOrEmpty(script_file))
            {
                throw new Exception($"{_config.ContextAsBackdropPromptFile} Prompt File is not set in the configuration.");
            }
            var script_file_text = File.ReadAllText(Path.Combine("./Prompt", script_file));
            var prompt = Template.Parse(script_file_text);
            return Task.FromResult(prompt.Render(new { context }));
        }
    }
}
