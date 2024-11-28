using Newtonsoft.Json;

namespace ClaudeApi.Agents.ContextCore
{
    [JsonConverter(typeof(ContextConverter))]
    public class Context : IContext
    {
        public static IContext Empty { get; } = new Context("Empty", "Empty");
        public IContext? Parent { get; private set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public List<IContext> SubContexts { get; private set; } = [];

        public Context(string summary, string details, IContext? parent = null)
        {
            Summary = summary;
            Details = details;
            Parent = parent;
        }

        public void AddSubContext(IContext subContext)
        {
            ArgumentNullException.ThrowIfNull(subContext);

            subContext.SetParent(this);
            SubContexts.Add(subContext);
        }

        public void RemoveSubContext(IContext subContext)
        {
            ArgumentNullException.ThrowIfNull(subContext);

            if (SubContexts.Remove(subContext))
            {
                subContext.SetParent(null);
            }
        }

        void IContext.SetParent(IContext? parent)
        {
            Parent = parent;
        }

        public static Context? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Context>(json);
        }
    }
}
