using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.ContextCore
{
    internal class BaseContextualizeResponse
    {
        public Context? GeneratedContext { get; set; }
        public List<string>? UncapturedInformation { get; set; }
    }
}
