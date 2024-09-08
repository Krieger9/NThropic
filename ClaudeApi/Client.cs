using ClaudeApi.Tools;
using ClaudeApi.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using NJsonSchema.Generation;
using System.IO;
using System.Threading.Channels;
using ClaudeApi.Prompts;
using Scriban;

namespace ClaudeApi
{
    public partial class Client
    {
        private readonly string _ephemeralCacheControl = "{\"type\": \"ephemeral\"}";
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<Client> _logger;
        private readonly ToolDiscoveryService _toolDiscoveryService;
        private readonly ToolExecutionService _toolExecutionService;
        private readonly List<Tool> _discoveredTools;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<string> _contextFiles = [];
        private readonly string _promptsFolder;

        public List<ContentBlock> DefaultSystemMessage { get; set; } = [ContentBlock.FromString("a helpful assistant")];

        public Client(IConfiguration configuration, ILogger<Client> logger, System.Reflection.Assembly toolAssembly, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _apiKey = configuration["ClaudeApiKey"] ?? throw new InvalidOperationException("API key is not configured.");
            _promptsFolder = configuration["PromptsFolder"] ?? "./Prompts";

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

            _toolDiscoveryService = new ToolDiscoveryService();
            _discoveredTools = _toolDiscoveryService.DiscoverTools(toolAssembly);
            _toolExecutionService = new ToolExecutionService(_discoveredTools);
            _serviceProvider = serviceProvider;

            _logger.LogInformation("Client initialized with {ToolCount} discovered tools", _discoveredTools.Count);
        }

        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
         List<Message> initialMessages,
         List<ContentBlock>? systemMessage = null,
         string model = "claude-3-5-sonnet-20240620",
         int maxTokens = 1024,
         double temperature = 1.0)
        {
            var channel = Channel.CreateUnbounded<string>();

            _ = ProcessConversationInternalAsync(channel.Writer, initialMessages, systemMessage, model, maxTokens, temperature);

            return channel.Reader.ReadAllAsync();
        }

        // New overload that takes a string and History
        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<Message> history,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0)
        {
            var userMessage = new Message
            {
                Role = "user",
                Content = [ContentBlock.FromString(userInput)]
            };

            history.Add(userMessage);

            return ProcessContinuousConversationAsync(history, systemMessage, model, maxTokens, temperature);
        }

        // New overload that takes only a string
        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0)
        {
            var history = new List<Message>();
            var userMessage = new Message
            {
                Role = "user",
                Content = [ ContentBlock.FromString(userInput) ]
            };

            history.Add(userMessage);

            return ProcessContinuousConversationAsync(history, systemMessage, model, maxTokens, temperature);
        }
        public async Task<IAsyncEnumerable<string>> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0)
        {
            var userMessage = await ParsePromptAsync(prompt);
            history.Add(userMessage);

            return ProcessContinuousConversationAsync(history, systemMessage, model, maxTokens, temperature);
        }

        private async Task<Message> ParsePromptAsync(Prompt prompt)
        {
            var filePath = Path.Combine(_promptsFolder, $"{prompt.Name}.scriban");
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
                Content = [ ContentBlock.FromString(renderedContent) ]
            };
        }

        private async Task ProcessConversationInternalAsync(
            ChannelWriter<string> writer,
            List<Message> messages,
            List<ContentBlock>? systemMessage,
            string model,
            int maxTokens,
            double temperature)
        {
            try
            {
                while (true)
                {
                    var apiCall = CreateMessageStreamAsyncInternal(messages, systemMessage ?? DefaultSystemMessage, model, maxTokens, temperature);
                    var toolCompletionQueue = new BlockingCollection<ToolResult>();
                    var sseProcessor = new SseProcessor(_serviceProvider.GetRequiredService<ILogger<SseProcessor>>());

                    sseProcessor.OnToolUseCompleted += async (toolUse) =>
                    {
                        var message = new Message
                        {
                            Role = "assistant",
                            Content = [
                                new ToolUseContentBlock
                                {
                                    Id = toolUse.Id,
                                    Name = toolUse.ToolName,
                                    Input = toolUse.Input
                                }
                            ]
                        };
                        messages.Add(message);
                        var result = await ExecuteToolAsync(toolUse);
                        toolCompletionQueue.Add(result);
                    };

                    sseProcessor.OnMessageStop += (messageResponse) =>
                    {
                    };

                    var response = await apiCall;
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();

                    await foreach (var message in sseProcessor.ProcessStreamAsync(stream))
                    {
                        await writer.WriteAsync(message);
                    }

                    if (toolCompletionQueue.Count > 0)
                    {
                        while (toolCompletionQueue.TryTake(out var toolResult))
                        {
                            var toolResultMessage = CreateToolResultMessage(toolResult);
                            messages.Add(toolResultMessage);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                writer.Complete();
            }
        }

        private async Task<ToolResult> ExecuteToolAsync(ToolUse toolUse)
        {
            try
            {
                var result = await _toolExecutionService.ExecuteToolAsync(toolUse.ToolName, toolUse.Input);
                return new ToolResult(toolUse.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolUse.ToolName);
                return new ToolResult(toolUse.Id, $"Error: {ex.Message}", isError: true);
            }
        }

        private async Task<HttpResponseMessage> CreateMessageStreamAsyncInternal(
            List<Message> messages,
            List<ContentBlock> systemMessage,
            string model,
            int maxTokens,
            double temperature)
        {
            var use_tools = _discoveredTools.Select(t => new MessagesRequest.ToolInfo
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema ?? throw new InvalidOperationException($"{nameof(t.InputSchema)} cannot be null.")
            });
            use_tools.Last().CacheControl = _ephemeralCacheControl;

            var use_system_message = systemMessage.Last().CacheControl = _ephemeralCacheControl;

            var contextBlocks = CreateContextBlocks();
            systemMessage.AddRange(contextBlocks);

            var request = new MessagesRequest
            {
                Model = model,
                SystemMessage = systemMessage,
                Messages = messages,
                MaxTokens = maxTokens,
                Temperature = temperature,
                Stream = true,
                Tools = use_tools.ToList()
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Remove("anthropic-version");
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Remove("anthropic-beta");
            _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");

            _logger.LogInformation("Sending streaming request to Claude API. Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}", model, maxTokens, temperature);

            return await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
        }

        private List<ContentBlock> CreateContextBlocks()
        {
            var contextBlocks = new List<ContentBlock>();

            foreach (var filePath in _contextFiles)
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Context file not found: {FilePath}", filePath);
                    continue;
                }

                try
                {
                    var fileContent = File.ReadAllText(filePath);
                    var fileName = Path.GetFileName(filePath);

                    contextBlocks.Add(ContentBlock.FromString($"Content of file '{fileName}':", _ephemeralCacheControl));
                    contextBlocks.Add(ContentBlock.FromString(fileContent, _ephemeralCacheControl));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading context file: {FilePath}", filePath);
                }
            }

            // Only set the CacheControl for the last element if the list is not empty
            if (contextBlocks.Count > 0)
            {
                contextBlocks.Last().CacheControl = _ephemeralCacheControl;
            }

            return contextBlocks;
        }

        private static Message CreateToolResultMessage(ToolResult toolResult)
        {
            var message = new Message
            {
                Role = "user",
                Content = [
                    new ToolResultContentBlock
                    {
                        ToolUseId = toolResult.Id,
                        Content = toolResult.Result,
                    }
                ]
            };
            return message;
        }

        public void AddContextFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {

                throw new FileNotFoundException($"The file '{filePath}' does not exist.");
            }

            if (!_contextFiles.Contains(filePath))
            {
                _contextFiles.Add(filePath);
                _logger.LogInformation("Added context file: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Context file already added: {FilePath}", filePath);
            }
        }

        public IReadOnlyList<string> GetContextFiles()
        {
            return _contextFiles.AsReadOnly();
        }

        // Remove or comment out the old CreateMessageStreamAsync and CreateMessageStreamAsyncInternal methods
    }
}
