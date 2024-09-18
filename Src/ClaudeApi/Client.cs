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
using System.Net.Http;
using System.IO.Pipelines;
using System.Reflection;

namespace ClaudeApi
{
    public partial class Client
    {
        private readonly JObject _ephemeralCacheControl = JObject.Parse("{\"type\": \"ephemeral\"}");
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<Client> _logger;
        private readonly ToolDiscoveryService _toolDiscoveryService;
        private readonly ToolExecutionService _toolExecutionService;
        private readonly IToolRegistry _toolRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<string> _contextFiles = [];
        private readonly string _promptsFolder;
        private readonly ISandboxFileManager _sandboxFileManager;

        public List<ContentBlock> DefaultSystemMessage { get; set; } = [ContentBlock.FromString("a helpful assistant")];

        public Client(ISandboxFileManager sandboxFileManager, IToolRegistry toolRegistry, IConfiguration configuration, ILogger<Client> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _apiKey = configuration["ClaudeApiKey"] ?? throw new InvalidOperationException("API key is not configured.");
            _promptsFolder = configuration["PromptsFolder"] ?? "./Prompts";

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

            _serviceProvider = serviceProvider;
            _toolDiscoveryService = new ToolDiscoveryService(_serviceProvider);
            _toolRegistry = toolRegistry;
            _toolExecutionService = new ToolExecutionService(_toolRegistry);

            _sandboxFileManager = sandboxFileManager;

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
            List<ContentBlock> systemMessage,
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

            if (use_tools.Any())
            {
                use_tools.Last().CacheControl = _ephemeralCacheControl;
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

            _httpClient.DefaultRequestHeaders.Remove("anthropic-version");
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Remove("anthropic-beta");
            _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");

            _logger.LogInformation("Sending streaming request to Claude API. Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}", model, maxTokens, temperature);

            var pipe = new Pipe();

            // Use StreamContent and provide the pipe reader stream.
            var content = new StreamContent(pipe.Reader.AsStream());

            // Start writing data to the pipe.
            _ = Task.Run(async () =>
            {
                await using (var writer = new StreamWriter(pipe.Writer.AsStream(), new UTF8Encoding(false)))
                await using (var jsonWriter = new JsonTextWriter(writer))
                {
                    var serializer = new JsonSerializer();
                    jsonWriter.WriteStartObject();

                    // Write the request properties
                    jsonWriter.WritePropertyName("model");
                    jsonWriter.WriteValue(request.Model);

                    jsonWriter.WritePropertyName("system");

                    // Collect and concatenate file content messages into a single string
                    var fileContentBuilder = new StringBuilder();
                    for (var i = 0; i < _contextFiles.Count; i++)
                    {
                        var filePath = _contextFiles[i];
                        if (_sandboxFileManager.FileExists(filePath))
                        {
                            await using var fileStream = await _sandboxFileManager.OpenReadAsync(filePath);
                            using var fileReader = new StreamReader(fileStream);
                            string fileContent = await fileReader.ReadToEndAsync();
                            fileContentBuilder.AppendLine($"### {Path.GetFileName(filePath)} ###");
                            fileContentBuilder.AppendLine(fileContent);

                            if (i < _contextFiles.Count - 1)
                            {
                                fileContentBuilder.AppendLine();
                            }
                        }
                    }

                    // Make a copy of systemMessage for temporary use
                    var tempSystemMessage = new List<ContentBlock>(request.SystemMessage);

                    // Add the concatenated content to the system message if there is any content
                    if (fileContentBuilder.Length > 0)
                    {
                        tempSystemMessage.Add(ContentBlock.FromString(fileContentBuilder.ToString(), _ephemeralCacheControl));
                    }

                    serializer.Serialize(jsonWriter, tempSystemMessage);

                    jsonWriter.WritePropertyName("messages");
                    jsonWriter.WriteStartArray();

                    // Write the existing messages
                    for (int i = 0; i < request.Messages.Count; i++)
                    {
                        var message = request.Messages[i];
                        if (i == request.Messages.Count - 1 && message?.Content?.Count > 0)
                        {
                            message.Content.Last().CacheControl = _ephemeralCacheControl;
                        }
                        serializer.Serialize(jsonWriter, message);
                    }

                    jsonWriter.WriteEndArray();

                    // Write the remaining request properties
                    jsonWriter.WritePropertyName("max_tokens");
                    jsonWriter.WriteValue(request.MaxTokens);

                    jsonWriter.WritePropertyName("temperature");
                    jsonWriter.WriteValue(request.Temperature);

                    jsonWriter.WritePropertyName("stream");
                    jsonWriter.WriteValue(request.Stream);

                    jsonWriter.WritePropertyName("tools");
                    serializer.Serialize(jsonWriter, request.Tools);

                    jsonWriter.WriteEndObject();
                }

                // Complete the pipe so the reader knows no more data is coming.
                pipe.Writer.Complete();
            });

            return await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
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
