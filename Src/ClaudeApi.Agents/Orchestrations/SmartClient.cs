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
                        { CHALLENGE_LEVEL.AUTO, ("auto-model", 4096, 0.0) },
                        { CHALLENGE_LEVEL.NONE, ("none-model", 512, 0.0) },
                        { CHALLENGE_LEVEL.ELEMENTARY, ("claude-3-haiku-20240307", 1024, 0.0) },
                        { CHALLENGE_LEVEL.INTERMEDIATE, ("claude-3-5-haiku-20241022", 2048, 0.0) },
                        { CHALLENGE_LEVEL.PROFESSIONAL, ("claude-3-5-sonnet-20241022", 4096, 0.0) },
                        { CHALLENGE_LEVEL.EXPERT, ("claude-3-5-sonnet-20241022", 4096, 0.0) },
                        { CHALLENGE_LEVEL.LEADING_EXPERT, ("claude-3-5-sonnet-20241022", 4096, 0.0) }
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

        public async Task<(string, string)> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            var result = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage, model, maxTokens, temperature);
            return result; 
        }

//        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
//            List<Message> initialMessages,
//            CHALLENGE_LEVEL challengeLevel,
//            string model,
//            int maxTokens,
//            double temperature)
//        {
//            return _client.ProcessContinuousConversationAsync(initialMessages, null, model, maxTokens, temperature);
//        }
//
//        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
//            string userInput,
//            List<Message> history,
//            CHALLENGE_LEVEL challengeLevel,
//            string model,
//            int maxTokens,
//            double temperature)
//        {
//            return _client.ProcessContinuousConversationAsync(userInput, history, null, model, maxTokens, temperature);
//        }
//
//        public IAsyncEnumerable<string> ProcessContinuousConversationAsync(
//            string userInput,
//            CHALLENGE_LEVEL challengeLevel,
//            string model,
//            int maxTokens,
//            double temperature)
//        {
//            return _client.ProcessContinuousConversationAsync(userInput, null, model, maxTokens, temperature);
//        }
//
//        public async Task<(IAsyncEnumerable<string>, string)> ProcessContinuousConversationAsync(
//            Prompt prompt,
//            List<Message> history,
//            CHALLENGE_LEVEL challengeLevel,
//            string model,
//            int maxTokens,
//            double temperature)
//        {
//            return await _client.ProcessContinuousConversationAsync(prompt, history, null, model, maxTokens, temperature);
//        }

        public void AddContextFile(string filePath) => _client.AddContextFile(filePath);
        public IReadOnlyList<string> GetContextFiles() => _client.GetContextFiles();
    }
}
