using ClaudeApi.Agents.Agents.Configs;
using ClaudeApi.Agents.Orchestrations;
using Microsoft.Extensions.Options;

namespace ClaudeApi.Agents.Agents
{
    public class ContextualizeAgent : Agent
    {
        private readonly ContextualizeAgentConfig _config;
        private readonly SummaryAgent _summarryAgent;

        public ContextualizeAgent(IOptions<ContextualizeAgentConfig> config, SummaryAgent summaryAgent)
        {
            _config = config.Value;
            _summarryAgent = summaryAgent;
        }

        public async Task<IContext> Contextualize(IEnumerable<IEnumerable<ExecutableResponse>> asks)
        {
            var information = asks.SelectMany(a => a.Where(r => r.Response is string).Select(r => r.Response as string)).Aggregate((a, b) => $"<Question>{a}</Question><Answer>{b}</Answer>\n");
            if (information == null)
            {
                return Context.Empty;
            }
            return await ContextualizeInternal(information);
        }

        public async Task<IContext> ContextualizeInternal(string information)
        {
            return await Task.FromResult(Context.Empty);
        }

        private async Task<string> GetSummary(string information)
        {
            var wordCount = information.Split(' ').Length;
            if (wordCount <= _config.SummaryLengthThreshold)
            {
                return information;
            }

            return await Task.FromResult(information);
        }
    }
}
