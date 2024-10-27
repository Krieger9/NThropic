using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public partial class OrchestrationAgent(IRequestExecutor executor) : Agent
    {
        public async Task<string> ExecuteAsync(string input, WorkItem work_item)
        {
            var plan = executor
                .Ask(new Prompt("ChallengeLevelAssesement"))
                .Ask(new Prompt("BreakdownTask")
                {
                    Arguments = new Dictionary<string, object>{{"task", input}}
                });

            var run = await plan.ExecuteAsync();
            var result = await run.AsAsync<string>();
            return result ?? "No response received.";
        }
    }
}
