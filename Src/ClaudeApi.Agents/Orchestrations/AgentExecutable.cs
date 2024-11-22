namespace ClaudeApi.Agents.Orchestrations
{
    public class AgentExecutable : IExecute
    {
        public IAgent? Agent { get; set; }
        public async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if (Agent == null)
            {
                throw new Exception($"Agent is not set {nameof(AgentExecutable)}");
            }
            return await Agent.ExecuteAsync(requestExtractor.InformationString, []);
        }
    }
}
