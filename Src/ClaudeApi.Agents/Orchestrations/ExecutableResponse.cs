using ClaudeApi.Agents.Orchestrations;

public class ExecutableResponse
{
    public IExecute Executable { get; set; }
    public string Response { get; set; }

    public ExecutableResponse(IExecute executable)
    {
        Executable = executable;
        Response = string.Empty;
    }
}
