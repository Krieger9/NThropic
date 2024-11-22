using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    public class Contextualize : IExecute, IObjectValue
    {
        public object? ObjectValue { get; private set; }

        public virtual async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            requestExtractor.Context = await requestExtractor.ContextualizeAgent.Contextualize(requestExtractor.Information, requestExtractor.Context);
            ObjectValue = requestExtractor.Context;
            return await Task.FromResult(string.Empty);
        }

        public override string ToString()
        {
            return ObjectValue?.ToString() ?? "";
        }
    }
}
