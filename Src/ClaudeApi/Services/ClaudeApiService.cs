using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using ClaudeApi.Messages;
using Microsoft.Extensions.Configuration;

namespace ClaudeApi.Services
{
    public class ClaudeApiService: IClaudeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClaudeApiService> _logger;
        private readonly ISandboxFileManager _sandboxFileManager;
        private readonly JObject _ephemeralCacheControl = JObject.Parse("{\"type\": \"ephemeral\"}");
        private readonly string _apiKey;
        public List<ContentBlock> DefaultSystemMessage { get; set; } = [ContentBlock.FromString("a helpful assistant")];

        public ClaudeApiService(
            HttpClient httpClient,
            ISandboxFileManager sandboxFileManager,
            ILogger<ClaudeApiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ClaudeApiKey"] ?? throw new InvalidOperationException("API key is not configured.");
            _httpClient.DefaultRequestHeaders.Remove("x-api-key");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Remove("anthropic-version");
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Remove("anthropic-beta");
            _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");

            _sandboxFileManager = sandboxFileManager;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> SendMessageAsync(
            MessagesRequest request,
            List<Message> messages,
            List<ContentBlock>? systemMessage = null,
            List<string>? _contextFiles = null)
        {


            _logger.LogInformation("Sending streaming request to Claude API. Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
                request.Model, request.MaxTokens, request.Temperature);

            var pipe = new Pipe();

            // Use StreamContent and provide the pipe reader stream.
            var content = new StreamContent(pipe.Reader.AsStream());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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
                    for (var i = 0; i < _contextFiles?.Count; i++)
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
                    var tempSystemMessage = new List<ContentBlock>(systemMessage ?? DefaultSystemMessage);

                    // Add the concatenated content to the system message if there is any content
                    if (fileContentBuilder.Length > 0)
                    {
                        tempSystemMessage.Add(ContentBlock.FromString(fileContentBuilder.ToString(), _ephemeralCacheControl));
                    }

                    serializer.Serialize(jsonWriter, tempSystemMessage);

                    jsonWriter.WritePropertyName("messages");
                    jsonWriter.WriteStartArray();

                    // Write the existing messages
                    for (int i = 0; i < messages.Count; i++)
                    {
                        var message = messages[i];
                        if (i == messages.Count - 1 && message?.Content?.Count > 0)
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
    }
}
