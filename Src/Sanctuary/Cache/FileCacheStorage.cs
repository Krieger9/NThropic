using System.Text.Json;

namespace Sanctuary.Cache
{
    public class FileCacheStorage<TKey, TValue> : ICacheStorage<TKey, TValue> where TKey : notnull
    {
        private readonly string _directory;
        private readonly string _fileExtension;

        public FileCacheStorage(string directory = "cache", string fileExtension = ".cache")
        {
            _directory = directory;
            _fileExtension = fileExtension;
            Directory.CreateDirectory(_directory);
        }

        private string GetFilePath(TKey key) =>
            Path.Combine(_directory, $"{key.GetHashCode()}{_fileExtension}");

        public async Task SaveAsync(CacheItem<TKey, TValue> item)
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(GetFilePath(item.Key), json);
        }

        public async Task<IEnumerable<CacheItem<TKey, TValue>>> LoadValidItemsAsync()
        {
            var validItems = new List<CacheItem<TKey, TValue>>();

            foreach (var file in Directory.GetFiles(_directory, $"*{_fileExtension}"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var item = JsonSerializer.Deserialize<CacheItem<TKey, TValue>>(json);

                    if (item?.IsValid == true)
                    {
                        validItems.Add(item);
                    }
                    else
                    {
                        // Clean up expired items
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    // Handle corrupted cache files by removing them
                    File.Delete(file);
                }
            }

            return validItems;
        }

        public async Task RemoveAsync(TKey key)
        {
            var path = GetFilePath(key);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }
    }
}