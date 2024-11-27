using ClaudeApi.Cache;
using ClaudeApi.Messages;

namespace ClaudeApi.Agents.Cache
{
    public interface IRequestCacheStorage
    {
        Task SaveAsync(CacheItem<ICacheKey, string> item);
        Task<IEnumerable<CacheItem<ICacheKey, string>>> LoadValidItemsAsync();
        Task RemoveAsync(ICacheKey key);
    }
}