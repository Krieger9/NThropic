using ClaudeApi.Agents.Cache;
using ClaudeApi.Cache;
using Newtonsoft.Json;

namespace Sanctuary.Cache
{
    public class FileCacheStorage : IRequestCacheStorage
    {
        private readonly string _directory;
        private readonly string _fileExtension;
        private readonly JsonSerializerSettings _jsonSettings;

        public FileCacheStorage(string directory = "cache", string fileExtension = ".cache")
        {
            _directory = directory;
            _fileExtension = fileExtension;
            Directory.CreateDirectory(_directory);

            _jsonSettings = new JsonSerializerSettings
            {
                Converters = { new CacheKeyConverter() }
            };
        }

        private string GetFilePath(ICacheKey key) =>
            Path.Combine(_directory, $"{key.GetHashCode()}{_fileExtension}");

        public async Task SaveAsync(CacheItem<ICacheKey, string> item)
        {
            var json = JsonConvert.SerializeObject(item, _jsonSettings);
            await File.WriteAllTextAsync(GetFilePath(item.Key), json);
        }

        public async Task<IEnumerable<CacheItem<ICacheKey, string>>> LoadValidItemsAsync()
        {
            var validItems = new List<CacheItem<ICacheKey, string>>();

            foreach (var file in Directory.GetFiles(_directory, $"*{_fileExtension}"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var item = JsonConvert.DeserializeObject<CacheItem<ICacheKey, string>>(json, _jsonSettings);

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

        public async Task RemoveAsync(ICacheKey key)
        {
            var path = GetFilePath(key);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }
    }
}