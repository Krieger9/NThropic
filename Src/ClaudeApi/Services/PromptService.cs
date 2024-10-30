using Scriban;
using Microsoft.Extensions.Logging;
using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using Microsoft.Extensions.Configuration;

namespace ClaudeApi.Services
{
    public class PromptService : IPromptService
    {
        private readonly string _promptsFolder;
        private readonly ILogger<PromptService> _logger;

        public PromptService(IConfiguration configuration, ILogger<PromptService> logger)
        {
            _promptsFolder = configuration["PromptsFolder"] ?? "./Prompts";
            _logger = logger;
        }

        public async Task<Message> ParsePromptAsync(Prompt prompt)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, _promptsFolder, $"{prompt.Name}.scriban");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The prompt file '{filePath}' does not exist.");
            }

            var templateContent = await File.ReadAllTextAsync(filePath);
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
            {
                throw new InvalidOperationException($"Errors in template: {string.Join(", ", template.Messages.Select(m => m.Message))}");
            }

            var renderedContent = template.Render(prompt.Arguments);
            return new Message
            {
                Role = "user",
                Content = [ContentBlock.FromString(renderedContent)]
            };
        }
    }
}
