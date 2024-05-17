using Microsoft.Extensions.Caching.Memory;

namespace Helpers.AspNetCore;

public static class IMemoryCacheExtensions
{
    public static IDictionary<string, T?> GetAll<T>(
        this IMemoryCache self,
        params string[] keys)
            where T : class
    {
        IDictionary<string, T?> map = new Dictionary<string, T?>();

        foreach (string key in keys)
        {
            map[key] = self.Get<T>(key);
        }

        return map;
    }

    public static void SetAll<T>(
        this IMemoryCache self,
        IDictionary<string, T> values,
        MemoryCacheEntryOptions? options = null)
    {
        var entries = new List<T>();

        foreach (var entry in values)
        {
            entries.Add(self.Set(entry.Key, entry.Value, options));
        }
    }
}