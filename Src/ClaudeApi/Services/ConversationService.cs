using System.Collections.Concurrent;
using ClaudeApi.Messages;
using Microsoft.Extensions.Logging;

namespace ClaudeApi.Services
{
    public class ConversationService
    {
        private readonly List<Message> _messages = new List<Message>();
        private readonly List<string> _contextFiles = new List<string>();
        private readonly ISandboxFileManager _sandboxFileManager;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(ISandboxFileManager sandboxFileManager, ILogger<ConversationService> logger)
        {
            _sandboxFileManager = sandboxFileManager;
            _logger = logger;
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
        }

        public IReadOnlyList<Message> GetMessages()
        {
            return _messages.AsReadOnly();
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
    }
}
