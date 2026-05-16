using Microsoft.Extensions.Caching.Memory;

namespace borealis_flowers.api.Features.Images;

public interface ICacheService
{
    bool Get<T>(string key, out T? value);
    void Set<T>(string key, T value);
    void Remove(string key);
}

public class ImageCacheService(IMemoryCache memoryCache) : ICacheService
{
    public bool Get<T>(string key, out T? value)
    {
        return memoryCache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value)
    {
        memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
    }
}
