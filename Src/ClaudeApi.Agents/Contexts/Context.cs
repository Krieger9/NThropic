using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Contexts
{
    public class Context
    {
        public Context? ParentContext { get; set; }
        public string? Details { get; set; }
        public string? Summary { get; set; }
        public List<Context>? SubContexts { get; set; }
    }
}
