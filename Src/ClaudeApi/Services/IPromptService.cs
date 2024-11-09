using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Services
{
    public interface IPromptService
    {
        Task<string> ParsePromptAsync(Prompt prompt);
    }
}
