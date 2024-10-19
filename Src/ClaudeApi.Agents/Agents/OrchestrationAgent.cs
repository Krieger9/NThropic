using ClaudeApi.Agents.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public partial class OrchestrationAgent : Agent
    {
        public async Task<string> ExecuteAsync(string input, WorkItem work_item)
        {
            return await Task.FromResult("Orchestration agent is running.");
        }
    }
}
