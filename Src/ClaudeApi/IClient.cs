using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System.Reactive.Subjects;
using System.Reflection;

namespace ClaudeApi
{
    public interface IClient
    {
        Subject<Usage> UsageSubject { get; }
        Subject<List<string>> ContextFilesSubject { get; }

        void DiscoverTools(Assembly toolAssembly);
        void DiscoverTools(Type type);
        void DiscoverTool(Type type, string methodName);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            List<Message> initialMessages,
            List<ContentBlock>? systemMessage = null,
            string model = "claude-3-5-sonnet-20240620",
            int maxTokens = 1024,
            double temperature = 1.0,
            string stop_sequence="");

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

        Task<(string response, string resolvedPrompt)> ProcessContinuousConversationAsync(
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