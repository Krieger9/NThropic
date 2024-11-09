namespace ClaudeApi.Agents.Orchestrations
{
    public interface IContext
    {
        IContext? Parent { get; }
        string Summary { get; }
        string Details { get; }
        List<IContext> SubContexts { get; }

        void AddSubContext(IContext subContext);
        void RemoveSubContext(IContext subContext);

        void SetParent(IContext? parent);
    }
}