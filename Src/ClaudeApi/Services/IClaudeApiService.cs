using System.Net.Http;
using System.Threading.Tasks;
using ClaudeApi.Messages;
using System.Collections.Generic;

namespace ClaudeApi.Services
{
    public interface IClaudeApiService
    {
        Task<HttpResponseMessage> SendMessageAsync(
            MessagesRequest request,
            List<Message> messages,
            List<ContentBlock>? systemMessage = null,
            List<string>? contextFiles = null);
    }
}
