namespace Sanctuary.Cache
{
    public interface ICacheStorage<TKey, TValue>
    {
        Task SaveAsync(CacheItem<TKey, TValue> item);
        Task<IEnumerable<CacheItem<TKey, TValue>>> LoadValidItemsAsync();
        Task RemoveAsync(TKey key);
    }
}