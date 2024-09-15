using ClaudeApi.Agents.Agents;
using ClaudeApi.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Tools
{
    public class SummarizeTools(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        [Tool("summarize", "Summarizes the given text with a specified tone and summary length")]
        public async Task<string> SummarizeTextAsync(string text, string tone, int summaryLength)
        {
            var summaryAgent = _serviceProvider.GetRequiredService<SummaryAgent>();

            var arguments = new Dictionary<string, object>
                {
                    { "tone", tone },
                    { "summary_length", summaryLength }
                };

            return await summaryAgent.ExecuteAsync(text, arguments);
        }
    }
}
