using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    public class Context : IContext
    {
        public IContext? Parent { get; private set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public List<IContext> SubContexts { get; private set; } = new List<IContext>();

        public Context(string summary, string details, IContext parent = null)
        {
            Summary = summary;
            Details = details;
            Parent = parent;
        }

        public void AddSubContext(IContext subContext)
        {
            if (subContext == null)
                throw new ArgumentNullException(nameof(subContext));

            subContext.SetParent(this);
            SubContexts.Add(subContext);
        }

        public void RemoveSubContext(IContext subContext)
        {
            if (subContext == null)
                throw new ArgumentNullException(nameof(subContext));

            if (SubContexts.Remove(subContext))
            {
                subContext.SetParent(null);
            }
        }

        void IContext.SetParent(IContext? parent)
        {
            Parent = parent;
        }
    }
}
