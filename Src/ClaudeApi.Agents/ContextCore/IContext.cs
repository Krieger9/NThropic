using Newtonsoft.Json;

namespace ClaudeApi.Agents.ContextCore
{
    [JsonConverter(typeof(ContextConverter))]
    public interface IContext
    {
        IContext? Parent { get; }
        string Title { get; }
        string Summary { get; }
        string Details { get; }
        List<IContext> SubContexts { get; }

        void AddSubContext(IContext subContext);
        void RemoveSubContext(IContext subContext);

        void SetParent(IContext? parent);
    }
}