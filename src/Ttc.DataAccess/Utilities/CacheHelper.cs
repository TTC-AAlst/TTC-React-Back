using Microsoft.Extensions.Caching.Memory;

namespace Ttc.DataAccess.Utilities;

public class CacheHelper
{
    private readonly IMemoryCache _cache;

    public CacheHelper(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan expiration)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration;
            var lazyValue = new Lazy<T>(valueFactory, isThreadSafe: true);
            return lazyValue.Value;
        })!;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}