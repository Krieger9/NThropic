using ClaudeApi.Agents.Agents;
using ClaudeApi.Tools;

namespace ClaudiaCore.Tools
{
    public class Summarize
    {
        private readonly SummaryAgent _summaryAgent;

        // Constructor to inject SummaryAgent
        public Summarize(SummaryAgent summaryAgent)
        {
            _summaryAgent = summaryAgent ?? throw new ArgumentNullException(nameof(summaryAgent));
        }

        // The Tool attribute provides metadata for the SummarizeText method.
        // Parameters:
        // "summarize" - This is the command name or identifier for the tool. It indicates that the method performs a summarization action.
        // "Summarizes the given text with a specified tone and summary length" - This description explains that the method summarizes the provided text, taking into account the specified tone and summary length.
        [Tool("summarize", "Summarizes the given text with a specified tone and summary length")]
        public async Task<string> SummarizeTextAsync(string text, string tone, int summaryLength)
        {
            var arguments = new Dictionary<string, object>
                {
                    { "tone", tone },
                    { "summaryLength", summaryLength }
                };

            return await _summaryAgent.ExecuteAsync(text, arguments);
        }
    }
}
