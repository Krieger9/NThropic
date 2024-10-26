using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    public interface ISmartClient 
    {
        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            List<Message> initialMessages,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null);

        IAsyncEnumerable<string> ProcessContinuousConversationAsync(
            string userInput,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null);

        Task<IAsyncEnumerable<string>> ProcessContinuousConversationAsync(
            Prompt prompt,
            List<Message> history,
            CHALLENGE_LEVEL challengeLevel,
            List<ContentBlock>? systemMessage = null);
    }
}
