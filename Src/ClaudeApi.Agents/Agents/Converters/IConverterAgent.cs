using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents.Converters
{
    public interface IConverterAgent : IAgent
    {
        public Task<object?> ConvertToAsync(string input, Type desiredType);
    }
}
