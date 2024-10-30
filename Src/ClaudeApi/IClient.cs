using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System.Reflection;

namespace ClaudeApi
{
    public interface IClient
    {
        IObservable<Usage> UsageStream { get; }
        IObservable<List<string>> ContextFilesStream { get; }

        void DiscoverTools(Assembly toolAssembly);
        void DiscoverTools(Type type);
        void DiscoverTool(Type type, string methodName);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            List<Message> initialMessages,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<Message> history,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0);

        Task<IAsyncEnumerable<string>> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0);

        void AddContextFile(string filePath);
        IReadOnlyList<string> GetContextFiles();
    }
}