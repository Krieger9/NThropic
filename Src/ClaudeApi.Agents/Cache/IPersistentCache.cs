using ClaudeApi.Messages;
using System.Threading.Channels;

namespace ClaudeApi
{
    public interface IPersistentCache
    {
        void AddChannelListener(ICacheKey key, ChannelReader<string> reader, TimeSpan? ttl = null);
        bool Exists(ICacheKey key);
        Task InitializeAsync();
        Task RemoveAsync(ICacheKey key);
        Task SetAsync(ICacheKey key, string value, TimeSpan? ttl = null);
        Task<(bool exists, string? value)> TryGetAsync(ICacheKey key);
    }
}