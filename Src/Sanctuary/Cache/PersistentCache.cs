using ClaudeApi;
using ClaudeApi.Agents.Cache;
using ClaudeApi.Cache;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

namespace Sanctuary.Cache
{
    public class PersistentCache : IPersistentCache
    {
        private readonly IRequestCacheStorage _storage;
        private readonly ConcurrentDictionary<ICacheKey, CacheItem<ICacheKey, string>> _memoryCache;
        private readonly TimeSpan _defaultTtl;
        private readonly int _maxItems;
        private readonly object _cleanupLock = new();
        private DateTime _lastCleanup = DateTime.UtcNow;

        public bool Exists(ICacheKey key)
        {
            return _memoryCache.ContainsKey(key);
        }

        public PersistentCache(
            IRequestCacheStorage storage,
            TimeSpan? defaultTtl = null,
            int maxItems = 1000)
        {
            _storage = storage;
            _memoryCache = new ConcurrentDictionary<ICacheKey, CacheItem<ICacheKey, string>>();
            _defaultTtl = defaultTtl ?? TimeSpan.FromHours(1);
            _maxItems = maxItems;
        }

        public async Task InitializeAsync()
        {
            var validItems = await _storage.LoadValidItemsAsync();
            foreach (var item in validItems)
            {
                _memoryCache.TryAdd(item.Key, item);
            }
        }

        public void AddChannelListener(ICacheKey key, ChannelReader<string> reader, TimeSpan? ttl = null)
        {
            _ = Task.Run(async () =>
            {
                var value = new StringBuilder();
                await foreach (var item in reader.ReadAllAsync())
                {
                    value.Append(item);
                }
                if (value.Length > 0)
                {
                    await SetAsync(key, value.ToString(), ttl);
                }
            });
        }

        public async Task SetAsync(ICacheKey key, string value, TimeSpan? ttl = null)
        {
            var item = new CacheItem<ICacheKey, string>(
                key,
                value,
                DateTime.UtcNow.Add(ttl ?? _defaultTtl)
            );

            _memoryCache.AddOrUpdate(key, item, (_, _) => item);
            await _storage.SaveAsync(item);

            await CleanupIfNeededAsync();
        }

        public async Task<(bool exists, string? value)> TryGetAsync(ICacheKey key)
        {
            if (_memoryCache.TryGetValue(key, out var item) && item.IsValid)
            {
                return (true, item.Value);
            }

            // If item exists but is invalid, remove it
            if (item != null)
            {
                await RemoveAsync(key);
            }

            return (false, default);
        }

        public async Task RemoveAsync(ICacheKey key)
        {
            _memoryCache.TryRemove(key, out _);
            await _storage.RemoveAsync(key);
        }

        private async Task CleanupIfNeededAsync()
        {
            // Prevent multiple simultaneous cleanups
            if (!Monitor.TryEnter(_cleanupLock)) return;

            try
            {
                var now = DateTime.UtcNow;

                // Only cleanup if we've exceeded max items or it's been more than 5 minutes
                if (_memoryCache.Count > _maxItems || (now - _lastCleanup).TotalMinutes > 5)
                {
                    var expiredKeys = _memoryCache
                        .Where(kvp => !kvp.Value.IsValid)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        await RemoveAsync(key);
                    }

                    // If we're still over max items, remove oldest items
                    if (_memoryCache.Count > _maxItems)
                    {
                        var oldestKeys = _memoryCache
                            .OrderBy(kvp => kvp.Value.ExpiresAt)
                            .Take(_memoryCache.Count - _maxItems)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        foreach (var key in oldestKeys)
                        {
                            await RemoveAsync(key);
                        }
                    }

                    _lastCleanup = now;
                }
            }
            finally
            {
                Monitor.Exit(_cleanupLock);
            }
        }
    }
}