using ClaudeApi.Agents.ContextCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public interface IContextualizeAgent
    {
        Task<IContext?> Contextualize(List<string> information, IContext? currentContext = null);
        Task<IContext?> ContextualizeInternal(List<string> information, IContext? context);
        Task<string> GetBackdrop(IContext context);
    }
}
