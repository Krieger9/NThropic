using ClaudeApi.Agents.Orchestrations;

namespace ClaudeApi.Agents.Orchestrations
{
    public class ExecutableResponse
    {
        public IExecute Executable { get; set; }
        public Object? Response { get; set; }

        public ExecutableResponse(IExecute executable)
        {
            Executable = executable;
            Response = string.Empty;
        }
    }
}