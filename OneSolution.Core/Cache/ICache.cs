using System;
using System.Threading.Tasks;

namespace OneSolution.Core.Cache
{
    /// <summary>
    /// Cache service interface
    /// </summary>
    public interface ICache : IDisposable
    {
        T Get<T>(string key, Func<T> acquire, TimeSpan? cacheSpan = null);

        Task<T> Get<T>(string key, Func<Task<T>> acquire, TimeSpan? cacheSpan = null);

        void Set(string key, object data, TimeSpan cacheSpan);
        Task Set<T>(string key, Func<Task<T>> acquire, TimeSpan cacheSpan);

        bool IsSet(string key);

        void Remove(string key);

        int RemoveByPattern(string pattern);

        void Clear();
    }
}
