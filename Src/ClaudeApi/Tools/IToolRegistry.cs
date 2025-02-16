using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Tools
{
    public interface IToolRegistry
    {
        IEnumerable<Tool> Tools { get; }
        Dictionary<string, Tool> ValidateAndRegisterTools(List<Tool> tools);
        void AddTool(Tool tool);
        void AddTools(IEnumerable<Tool> tools);
        public void AddTools<T>(IEnumerable<Tool> tools, T instance) where T : class;
        void RemoveTool(string toolName);
        object GetOrCreateToolInstance(Tool tool);
        bool TryGetTool(string toolName, out Tool? tool);
    }
}
