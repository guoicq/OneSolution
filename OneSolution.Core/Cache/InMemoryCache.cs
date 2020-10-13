using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace OneSolution.Core.Cache
{
    public class InMemoryCache : ICache
    {
        public static readonly TimeSpan DefaultCacheSpan = TimeSpan.FromMinutes(5);

        protected static readonly ConcurrentDictionary<string, bool> keys;

        private readonly IMemoryCache cache;
        protected CancellationTokenSource cancellationTokenSource;

        static InMemoryCache()
        {
            keys = new ConcurrentDictionary<string, bool>();
        }

        public InMemoryCache(IMemoryCache cache)
        {
            this.cache = cache;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public virtual T Get<T>(string key, Func<T> acquire, TimeSpan? cacheSpan = null)
        {
            //item already is in cache, so return it
            if (cache.TryGetValue(key, out T value))
                return value;

            //or create it using passed function
            var result = acquire();

            //and set in cache (if cache time is defined)
            if (result != null)
            {
                Set(key, result, cacheSpan ?? DefaultCacheSpan);
            }

            return result;
        }

        public virtual async Task<T> Get<T>(string key, Func<Task<T>> acquire, TimeSpan? cacheSpan = null)
        {
            //item already is in cache, so return it
            if (cache.TryGetValue(key, out T value))
                return value;

            //or create it using passed function
            var result = await acquire().ConfigureAwait(false);

            //and set in cache (if cache time is defined)
            if (result != null)
            {
                Set(key, result, cacheSpan ?? DefaultCacheSpan);
            }

            return result;
        }

        public virtual void Set(string key, object data, TimeSpan cacheSpan)
        {
            if (data != null)
            {
                cache.Set(AddKey(key), data, GetMemoryCacheEntryOptions(cacheSpan));
            }
        }

        public virtual async Task Set<T>(string key, Func<Task<T>> acquire, TimeSpan cacheSpan)
        {
            var result = await acquire().ConfigureAwait(false);

            if (result != null)
            {
                cache.Set(AddKey(key), result, GetMemoryCacheEntryOptions(cacheSpan));
            }
        }

        public virtual bool IsSet(string key)
        {
            return cache.TryGetValue(key, out object _);
        }

        public virtual void Remove(string key)
        {
            cache.Remove(RemoveKey(key));
        }

        public virtual int RemoveByPattern(string pattern)
        {
            //get cache keys that matches pattern
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchesKeys = keys.Where(p => p.Value).Select(p => p.Key).Where(key => regex.IsMatch(key)).ToList();

            //remove matching values
            foreach (var key in matchesKeys)
            {
                cache.Remove(RemoveKey(key));
            }

            return matchesKeys.Count;
        }

        public virtual void Clear()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            cancellationTokenSource = new CancellationTokenSource();
        }

        public virtual void Dispose()
        {
            //nothing special
        }

        #region Private methods

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions(TimeSpan cacheTime)
        {
            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token))
                .RegisterPostEvictionCallback(PostEvict);

            //set cache time
            options.AbsoluteExpirationRelativeToNow = cacheTime;

            return options;
        }

        private string AddKey(string key)
        {
            keys.TryAdd(key, true);
            return key;
        }

        private string RemoveKey(string key)
        {
            TryRemoveKey(key);
            return key;
        }

        private void TryRemoveKey(string key)
        {
            //try to remove key from dictionary
            if (!keys.TryRemove(key, out _))
                //if not possible to remove key from dictionary, then try to mark key as not existing in cache
                keys.TryUpdate(key, false, true);
        }

        private void ClearKeys()
        {
            foreach (var key in keys.Where(p => !p.Value).Select(p => p.Key).ToList())
            {
                RemoveKey(key);
            }
        }

        private void PostEvict(object key, object value, EvictionReason reason, object state)
        {
            //if cached item just change, then nothing doing
            if (reason == EvictionReason.Replaced)
                return;

            //try to remove all keys marked as not existing
            ClearKeys();

            //try to remove this key from dictionary
            TryRemoveKey(key.ToString());
        }

        #endregion
    }

}
