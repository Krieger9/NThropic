using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using ClaudeApi.Services;
using ClaudeApi.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Channels;

namespace ClaudeApi
{
    public partial class ClaudeClient : IClient
    {
        private readonly JObject _ephemeralCacheControl = JObject.Parse("{\"type\": \"ephemeral\"}");
        private readonly ILogger<ClaudeClient> _logger;
        private readonly IToolManagementService _toolManagementService;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<string> _contextFiles = [];
        private readonly ISandboxFileManager _sandboxFileManager;
        private readonly IClaudeApiService _claudeApiService;
        private readonly IPromptService _promptService;

        private readonly Subject<Usage> _usageSubject = new();
        public IObservable<Usage> UsageStream => _usageSubject.AsObservable();

        private readonly Subject<List<string>> _contextFilesSubject = new();
        public IObservable<List<string>> ContextFilesStream => _contextFilesSubject.AsObservable();

        public ClaudeClient(ISandboxFileManager sandboxFileManager,
            IToolManagementService toolManagementService,
            IClaudeApiService claudeApiService,
            ILogger<ClaudeClient> logger,
            IPromptService promptService,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _toolManagementService = toolManagementService;
            _sandboxFileManager = sandboxFileManager;
            _claudeApiService = claudeApiService;
            _promptService = promptService;

            _logger.LogInformation("Client initialized.");
        }

        public void DiscoverTools(Assembly toolAssembly)
        {
            _toolManagementService.DiscoverTools(toolAssembly);
            _logger.LogInformation("Discovered {ToolCount} tools", _toolManagementService.ToolRegistry.Tools.Count());
        }

        public void DiscoverTools(Type type)
        {
            _toolManagementService.DiscoverTools(type);
            _logger.LogInformation("Discovered {ToolCount} tools from type {TypeName}", _toolManagementService.ToolRegistry.Tools.Count(), type.Name);
        }

        public void DiscoverTool(Type type, string methodName)
        {
            _toolManagementService.DiscoverTool(type, methodName);
            _logger.LogInformation("Discovered {ToolCount} total tools.", _toolManagementService.ToolRegistry.Tools.Count());
        }

        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
         List<Message> initialMessages,
         List<ContentBlock>? systemMessage = null,
         string model = "claude-3-5-sonnet-20240620",
         int maxTokens = 1024,
         double temperature = 1.0,
         string stop_sequence = "")
        {
            var channel = Channel.CreateUnbounded<string>();

            _ = ProcessConversationInternalAsync(channel.Writer, initialMessages, systemMessage, model, maxTokens, temperature, stop_sequence);

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

        public async Task<(string response, string resolvedPrompt)> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0)
        {
            var resolvedPrompt = await _promptService.ParsePromptAsync(prompt);
            var stop_sequence = "";

            var promptResponse = TryParsePromptResponse(resolvedPrompt);

            if (promptResponse != null)
            {
                var (filteredMessages, filteredSystemMessage) = FilterMessages(promptResponse);
                (model, maxTokens, temperature, stop_sequence) = OverrideParameters(promptResponse, model, maxTokens, temperature, stop_sequence);

                var result = await ProcessContinuousConversationAsync(filteredMessages, filteredSystemMessage, model, maxTokens, temperature, stop_sequence).ToSingleStringAsync();
                result = TrimResult(result, stop_sequence);

                return (result, resolvedPrompt);
            }
            else
            {
                var userMessage = CreateUserMessage(resolvedPrompt);
                history.Add(userMessage);

                var result = await ProcessContinuousConversationAsync(history, systemMessage, model, maxTokens, temperature).ToSingleStringAsync();
                return (result, resolvedPrompt);
            }
        }

        private PromptResponse? TryParsePromptResponse(string resolvedPrompt)
        {
            try
            {
                return JsonConvert.DeserializeObject<PromptResponse>(resolvedPrompt);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse resolvedPrompt as PromptResponse.");
                return null;
            }
        }

        private static (List<Message> filteredMessages, List<ContentBlock>? filteredSystemMessage) FilterMessages(PromptResponse promptResponse)
        {
            var filteredMessages = promptResponse.Messages.Where(m => m.Role == "user" || m.Role == "assistant").ToList();
            var filteredSystemMessage = promptResponse.Messages?.Where(m => m.Role == "system" && m.Content != null).SelectMany(m => m.Content!).ToList();
            return (filteredMessages, filteredSystemMessage);
        }

        private static (string model, int maxTokens, double temperature, string stop_sequence) OverrideParameters(PromptResponse promptResponse, string model, int maxTokens, double temperature, string stop_sequence)
        {
            if (promptResponse.Meta != null)
            {
                if (promptResponse.Meta.TryGetValue("model", out var modelValue))
                {
                    model = modelValue.ToString();
                }
                if (promptResponse.Meta.TryGetValue("maxTokens", out var maxTokensValue))
                {
                    maxTokens = int.Parse(maxTokensValue.ToString());
                }
                if (promptResponse.Meta.TryGetValue("temperature", out var temperatureValue))
                {
                    temperature = double.Parse(temperatureValue.ToString());
                }
                if (promptResponse.Meta.TryGetValue("stop_sequence", out var stopSequenceValue))
                {
                    stop_sequence = stopSequenceValue.ToString();
                }
            }
            return (model, maxTokens, temperature, stop_sequence);
        }

        private static string TrimResult(string result, string stop_sequence)
        {
            result = result.Trim();
            if (!string.IsNullOrWhiteSpace(result) && result.TrimEnd().EndsWith(stop_sequence))
            {
                result = result[..^stop_sequence.Length].TrimEnd();
            }
            return result;
        }

        private static Message CreateUserMessage(string resolvedPrompt)
        {
            return new Message
            {
                Role = "user",
                Content = [ContentBlock.FromString(resolvedPrompt)]
            };
        }

        private async Task ProcessConversationInternalAsync(
            ChannelWriter<string> writer,
            List<Message> messages,
            List<ContentBlock>? systemMessage,
            string model,
            int maxTokens,
            double temperature,
            string stop_sequence = "")
        {
            try
            {
                while (true)
                {
                    var apiCall = CreateMessageStreamAsyncInternal(messages, systemMessage, model, maxTokens, temperature, stop_sequence);
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
                        if (messageResponse?.Usage != null)
                        {
                            _usageSubject.OnNext(messageResponse.Usage);
                        }
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
                var result = await _toolManagementService.ExecuteToolAsync(toolUse.ToolName, toolUse.Input, this, messages);
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
            double temperature,
            string stop_sequence)
        {
            var use_tools = _toolManagementService.ToolRegistry.Tools.Select(t => new MessagesRequest.ToolInfo
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
                Tools = use_tools,
                StopSequences = [stop_sequence]
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
                _contextFilesSubject.OnNext(_contextFiles);
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
    }
}
