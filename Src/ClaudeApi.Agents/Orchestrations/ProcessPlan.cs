using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    public enum ProcessType
    {
        SINGULAR_EXECUTION,
        ITERATIVE_LIST_EXECUTION,
        PARALLEL_LIST_EXECUTION,
        ESCALATION_TO_USER
    }

    internal class ProcessPlan
    {
    }
}
