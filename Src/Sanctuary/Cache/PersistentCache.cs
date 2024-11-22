using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Sanctuary.Cache
{
    public class PersistentCache<TKey, TValue> where TKey : notnull
    {
        private readonly ICacheStorage<TKey, TValue> _storage;
        private readonly ConcurrentDictionary<TKey, CacheItem<TKey, TValue>> _memoryCache;
        private readonly TimeSpan _defaultTtl;
        private readonly int _maxItems;
        private readonly object _cleanupLock = new();
        private DateTime _lastCleanup = DateTime.UtcNow;

        public PersistentCache(
            ICacheStorage<TKey, TValue> storage,
            TimeSpan? defaultTtl = null,
            int maxItems = 1000)
        {
            _storage = storage;
            _memoryCache = new ConcurrentDictionary<TKey, CacheItem<TKey, TValue>>();
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

        public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
        {
            var item = new CacheItem<TKey, TValue>(
                key,
                value,
                DateTime.UtcNow.Add(ttl ?? _defaultTtl)
            );

            _memoryCache.AddOrUpdate(key, item, (_, _) => item);
            await _storage.SaveAsync(item);

            await CleanupIfNeededAsync();
        }

        public async Task<(bool exists, TValue? value)> TryGetAsync(TKey key)
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

        public async Task RemoveAsync(TKey key)
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