using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClaudeApi.Messages;
using ClaudeApi.Tools;

namespace ClaudeApi
{
    public class SseProcessor(ILogger<SseProcessor> logger)
    {
        private readonly Dictionary<int, ToolUse> _pendingToolUses = [];
        private MessagesResponse? CurrentMessage { get; set; }

        public event Func<ToolUse, Task>? OnToolUseCompleted;
        public event Action<string>? OnError;
        public event Action<MessagesResponse?>? OnMessageStop;

        public async IAsyncEnumerable<string> ProcessStreamAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var eventData = line.Remove(0, 6).Trim();
                var sseEvent = JsonConvert.DeserializeObject<SseEvent>(eventData);

                if (sseEvent == null)
                    continue;

                var result = await ProcessEvent(sseEvent);
                if (!string.IsNullOrEmpty(result))
                    yield return result;
            }
        }

        private async Task<string> ProcessEvent(SseEvent sseEvent)
        {
            switch (sseEvent.Type)
            {
                case "message_start":
                    CurrentMessage = sseEvent.Message;
                    break;
                case "content_block_start":
                    HandleContentBlockStart(sseEvent);
                    break;
                case "content_block_delta":
                    return HandleContentBlockDelta(sseEvent);
                case "content_block_stop":
                    await HandleContentBlockStop(sseEvent);
                    break;
                case "message_stop":
                    HandleMessageStop();
                    break;
                case "error":
                    HandleError(sseEvent);
                    break;
                case "message_delta":
                case "ping":
                    break;
                default:
                    logger.LogWarning("Unknown event type received: {EventType}", sseEvent.Type);
                    break;
            }
            return string.Empty;
        }

        private void HandleContentBlockStart(SseEvent sseEvent)
        {
            if (sseEvent.ContentBlock?.Type == "tool_use")
            {
                var content = sseEvent.ContentBlock.GetContent();
                if (sseEvent.ContentBlock?.GetContent() is not JObject)
                {
                    throw new JsonSerializationException("Tool use content is null");
                }
                if (content == null)
                {
                    throw new JsonSerializationException("Tool use content is null");
                }
                var id = content["id"]?.ToString();
                var name = content["name"]?.ToString();
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                {
                    _pendingToolUses[sseEvent.Index] = new ToolUse(id, name);
                    logger.LogInformation("Tool use started: {ToolName} (ID: {ToolId})", name, id);
                }
            }
        }

        private string HandleContentBlockDelta(SseEvent sseEvent)
        {
            if (sseEvent.Delta?.Type == "text_delta")
            {
                return sseEvent.Delta?.GetContent()?["text"]?.ToString() ?? string.Empty;
            }
            else if (sseEvent.Delta?.Type == "input_json_delta" &&
                     _pendingToolUses.TryGetValue(sseEvent.Index, out var toolUse))
            {
                var partialJson = sseEvent.Delta?.GetContent()?["partial_json"]?.ToString();
                if (partialJson != null)
                {
                    toolUse.AccumulateInput(partialJson);
                    logger.LogDebug("Accumulated input for tool {ToolName}: {PartialJson}",
                        toolUse.ToolName, partialJson);
                }
            }
            return string.Empty;
        }

        private async Task HandleContentBlockStop(SseEvent sseEvent)
        {
            if (_pendingToolUses.TryGetValue(sseEvent.Index, out var toolUse))
            {
                toolUse.CompleteInputAccumulation();
                _pendingToolUses.Remove(sseEvent.Index);
                logger.LogInformation("Tool use completed: {ToolName} (ID: {ToolId})",
                    toolUse.ToolName, toolUse.Id);
                var task = OnToolUseCompleted?.Invoke(toolUse);
                if(task != null)
                {
                    await task;
                }
            }
        }

        private void HandleMessageStop()
        {
            logger.LogInformation("Message stop received");
            OnMessageStop?.Invoke(CurrentMessage);
            CurrentMessage = null;
            _pendingToolUses.Clear();
        }

        private void HandleError(SseEvent sseEvent)
        {
            var errorMessage = sseEvent.Error?.Message ?? "Unknown error";
            logger.LogError("Error event received: {ErrorMessage}", errorMessage);
            OnError?.Invoke(errorMessage);
        }
    }
}