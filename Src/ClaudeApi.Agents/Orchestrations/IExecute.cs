namespace ClaudeApi.Agents.Orchestrations
{
    public interface IExecute
    {
        Task<string> ExecuteAsync(IRequestExecutor requestExtractor);
    }
}
