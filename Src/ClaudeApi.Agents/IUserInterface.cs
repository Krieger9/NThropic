using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents
{
    public interface IUserInterface
    {
        Task<string> PromptAsync(string message);
        void Message(string message);
        void ReceivePartialMessage(string partialMessage);
        void EndPartialMessage();
    }
}
