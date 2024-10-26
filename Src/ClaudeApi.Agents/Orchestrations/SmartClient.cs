using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    internal class SmartClient : ISmartClient
    {
        private readonly IClient _client;
        private readonly Dictionary<CHALLENGE_LEVEL, (string model, int maxTokens, double temperature)> _challengeLevelSettings;

        public SmartClient(IClient client)
        {
            _client = client;
            _challengeLevelSettings = new Dictionary<CHALLENGE_LEVEL, (string model, int maxTokens, double temperature)>
                {
                    { CHALLENGE_LEVEL.AUTO, ("auto-model", 1024, 1.0) },
                    { CHALLENGE_LEVEL.NONE, ("none-model", 512, 0.5) },
                    { CHALLENGE_LEVEL.ELEMENTARY, ("elementary-model", 256, 0.7) },
                    { CHALLENGE_LEVEL.INTERMEDIATE, ("intermediate-model", 512, 0.8) },
                    { CHALLENGE_LEVEL.PROFESSIONAL, ("professional-model", 1024, 0.9) },
                    { CHALLENGE_LEVEL.EXPERT, ("expert-model", 2048, 1.0) },
                    { CHALLENGE_LEVEL.LEADING_EXPERT, ("leading-expert-model", 4096, 1.2) }
                };
        }

        private (string model, int maxTokens, double temperature) GetSettings(CHALLENGE_LEVEL challengeLevel)
        {
            if (_challengeLevelSettings.TryGetValue(challengeLevel, out var settings))
            {
                return settings;
            }
            throw new ArgumentException($"Invalid challenge level: {challengeLevel}");
        }

        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            List<Message> initialMessages,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            return _client.ProcessContinuousConversationAsync(initialMessages, systemMessage, model, maxTokens, temperature);
        }

        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            return _client.ProcessContinuousConversationAsync(userInput, history, systemMessage, model, maxTokens, temperature);
        }

        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            return _client.ProcessContinuousConversationAsync(userInput, systemMessage, model, maxTokens, temperature);
        }

        public async Task<IAsyncEnumerable<string>> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            return await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage, model, maxTokens, temperature);
        }

        public void AddContextFile(string filePath) => _client.AddContextFile(filePath);
        public IReadOnlyList<string> GetContextFiles() => _client.GetContextFiles();
    }
}
