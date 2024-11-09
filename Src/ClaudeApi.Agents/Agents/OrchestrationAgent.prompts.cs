using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public partial class OrchestrationAgent : Agent
    {
        public override string SystemPrompt { get; set; } = "Better to remain silent and be thought a fool than to speak and to remove all doubt.";
    }
}
