using ClaudeApi.Agents.Orchestrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public class ContextualizeAgent : Agent
    {
        public async Task<IContext> Contextualize(IEnumerable<IEnumerable<ExecutableResponse>> asks)
        {
            var information = asks.SelectMany(a => a.Where(r => r.Response is string).Select(r => r.Response as string)).Aggregate((a, b) => $"<Question>{a}</Question><Answer>{b}</Answer>\n");
            if(information == null)
            {
                return new Context(information);
            }
            return await ContextualizeInternal(information);

        }
        public async Task<IContext> ContextualizeInternal(string information)
        {
            return await Task.FromResult(information);
        }
    }
}
