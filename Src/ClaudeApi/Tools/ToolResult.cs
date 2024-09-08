using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Tools
{
    public class ToolResult
    {
        public string Id { get; }
        public string Result { get; }
        public bool IsError { get; }

        public ToolResult(string id, string result, bool isError = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Result = result ?? throw new ArgumentNullException(nameof(result));
            IsError = isError;
        }
    }
}
