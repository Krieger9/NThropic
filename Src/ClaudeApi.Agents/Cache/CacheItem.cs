namespace ClaudeApi.Cache
{
    public class CacheItem<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsValid => DateTime.UtcNow <= ExpiresAt;

        public CacheItem(TKey key, TValue value, DateTime expiresAt)
        {
            Key = key;
            Value = value;
            ExpiresAt = expiresAt;
        }
    }
}