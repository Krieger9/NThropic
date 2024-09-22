using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using ClaudeApi.Services;
using ClaudeApi.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

namespace ClaudeApi
{
    public partial class Client
    {
        private readonly JObject _ephemeralCacheControl = JObject.Parse("{\"type\": \"ephemeral\"}");
        private readonly ILogger<Client> _logger;
        private readonly IToolDiscoveryService _toolDiscoveryService;
        private readonly IToolExecutionService _toolExecutionService;
        private readonly IToolRegistry _toolRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<string> _contextFiles = [];
        private readonly string _promptsFolder;
        private readonly ISandboxFileManager _sandboxFileManager;
        private readonly IClaudeApiService _claudeApiService;

        public Client(ISandboxFileManager sandboxFileManager, 
            IToolRegistry toolRegistry, 
            IClaudeApiService claudeApiService, 
            IConfiguration configuration, 
            ILogger<Client> logger, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _promptsFolder = configuration["PromptsFolder"] ?? "./Prompts";

            _serviceProvider = serviceProvider;
            _toolDiscoveryService = _serviceProvider.GetRequiredService<IToolDiscoveryService>();
            _toolRegistry = toolRegistry;
            _toolExecutionService = _serviceProvider.GetRequiredService<IToolExecutionService>();
            _toolRegistry = _toolExecutionService.ToolRegistry;

            _sandboxFileManager = sandboxFileManager;
            _claudeApiService = claudeApiService;

            _logger.LogInformation("Client initialized.");
        }

        public void DiscoverTools(Assembly toolAssembly)
        {
            var _discoveredTools = _toolDiscoveryService.DiscoverTools(toolAssembly);
            _toolRegistry.AddTools(_discoveredTools);
            _logger.LogInformation("Discovered {ToolCount} tools", _discoveredTools.Count);
        }

        public void DiscoverTools(Type type)
        {
            var tools = _toolDiscoveryService.DiscoverTools(type);
            _toolRegistry.AddTools(tools);
            _logger.LogInformation("Discovered {ToolCount} tools from type {TypeName}", tools.Count, type.Name);
        }

        public void DiscoverTool(Type type, string methodName)
        {
            var tool = _toolDiscoveryService.DiscoverTool(type, methodName);
            if (tool != null)
            {
                _toolRegistry.AddTool(tool);
                _logger.LogInformation("Discovered tool {ToolName} from type {TypeName}", tool.Name, type.Name);
            }
            else
            {
                _logger.LogWarning("No tool found with method name {MethodName} in type {TypeName}", methodName, type.Name);
            }
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
                Content = [ContentBlock.FromString(userInput)]
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

            var templateContent = await File.ReadAllTextAsync(filePath); var template = Template.Parse(templateContent);

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
                    var apiCall = CreateMessageStreamAsyncInternal(messages, systemMessage, model, maxTokens, temperature);
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

                        var result = await ExecuteToolAsync(toolUse, messages);
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

        private async Task<ToolResult> ExecuteToolAsync(ToolUse toolUse, List<Message> messages)
        {
            try
            {
                var result = await _toolExecutionService.ExecuteToolAsync(toolUse.ToolName, toolUse.Input, this, messages);
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
            List<ContentBlock>? systemMessage,
            string model,
            int maxTokens,
            double temperature)
        {
            var use_tools = _toolRegistry.Tools.Select(t => new MessagesRequest.ToolInfo
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema ?? throw new InvalidOperationException($"{nameof(t.InputSchema)} cannot be null.")
            }).ToList();

            if (use_tools.Count > 0)
            {
                var lastTool = use_tools.Last();
                if (lastTool.CacheControl == null)
                {
                    // Clear CacheControl for all tools
                    foreach (var tool in use_tools)
                    {
                        tool.CacheControl = null;
                    }
                    // Set CacheControl for the last tool
                    lastTool.CacheControl = _ephemeralCacheControl;
                }
            }

            var request = new MessagesRequest
            {
                Model = model,
                SystemMessage = systemMessage,
                Messages = messages,
                MaxTokens = maxTokens,
                Temperature = temperature,
                Stream = true,
                Tools = use_tools
            };

            return await _claudeApiService.SendMessageAsync(request, messages, systemMessage, _contextFiles);
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

            if (!_sandboxFileManager.FileExists(filePath))
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
