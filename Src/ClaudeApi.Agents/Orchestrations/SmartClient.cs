using System.Linq;
using System.Text;
using ClaudeApi.Messages;
using ClaudeApi.Prompts;

namespace ClaudeApi.Agents.Orchestrations
{
    internal class SmartClient : ISmartClient
    {
        private readonly IClient _client;
        private readonly IPersistentCache _requestCache;
        private readonly Dictionary<CHALLENGE_LEVEL, (string model, int maxTokens, double temperature)> _challengeLevelSettings;

        public SmartClient(IClient client, IPersistentCache requestCache)
        {
            _client = client;
            _requestCache = requestCache;
            InitializeAsync().GetAwaiter().GetResult();
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

        private async Task InitializeAsync()
        {
            await _requestCache.InitializeAsync();
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
            string userInput,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var newUserInput = new Message()
            {
                Role = "user",
                Content = [ContentBlock.FromString(userInput)]
            };
            return ProcessContinuousConversationAsync([newUserInput], challengeLevel, systemMessage);
        }

        public async IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            List<Message> initialMessages,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var key = new MessageCacheKey(initialMessages, systemMessage, challengeLevel);
            (var wasCacheHit, var cacheValue) = await _requestCache.TryGetAsync(key);

            if (wasCacheHit)
            {
                yield return cacheValue ?? "";
            }
            else {

                var (model, maxTokens, temperature) = GetSettings(challengeLevel);
                var result = _client.ProcessContinuousConversationAsync(initialMessages, systemMessage, model, maxTokens, temperature);

                StringBuilder compositeResult = new StringBuilder();

                await foreach(var item in result)
                {
                    compositeResult.Append(item);
                    yield return item;
                }
                await _requestCache.SetAsync(key, compositeResult.ToString());
            }
        }

        public async Task<(string, string)> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null)
        {
            var key = new PromptCacheKey(prompt);
            var cacheResult = await _requestCache.TryGetAsync(key);

            if (cacheResult.exists)
            {
                return (cacheResult.value, prompt.Name);
            }

            var (model, maxTokens, temperature) = GetSettings(challengeLevel);
            var result = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage, model, maxTokens, temperature);

            await _requestCache.SetAsync(key, result.response);

            return result;
        }

        public void AddContextFile(string filePath) => _client.AddContextFile(filePath);
        public IReadOnlyList<string> GetContextFiles() => _client.GetContextFiles();
    }
}
