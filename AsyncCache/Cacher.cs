using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace AsyncCache
{
    public static class Cacher
    {
        private static ConcurrentDictionary<string, object> keyLockDictionary = new ConcurrentDictionary<string, object>();
        private static CacheSettings settings = new CacheSettings();
        private static object settingsLock = new object();

        public static CacheSettings Settings
        {
            get { return settings; }
            set
            {
                lock (settingsLock)
                {
                    settings = value;
                }
            }
        }

        public static T Get<T>(string cacheKey, Func<T> dataProvider)
        {
            return Get(cacheKey, dataProvider, TimeSpan.FromMinutes(10));
        }

        public static T Get<T>(string cacheKey, Func<T> dataProvider, TimeSpan refreshIn)
        {
            object keyLockObj = LockForKey(cacheKey);

            CacheValue<T> value = GetCacheValueFor<T>(cacheKey);
            if (value == null)
            {
                lock (keyLockObj)
                {
                    value = GetCacheValueFor<T>(cacheKey);
                    if (value == null)
                    {
                        value = new CacheValue<T>()
                        {
                            CurrentState = CacheValueState.Loading,
                            ExpirationTime = GetRefreshTime(refreshIn)
                        };
                        MemoryCache.Default.Add(cacheKey, value, Clock.Now().Add(Settings.MaxTimeInCache));

                        value.Value = dataProvider();
                        value.CurrentState = CacheValueState.Live;
                        value.ExpirationTime = GetRefreshTime(refreshIn);
                    }
                }
            }
            else
            {
                if (value.CurrentState == CacheValueState.Loading)
                {
                    lock (keyLockObj)
                    {
                        value = GetCacheValueFor<T>(cacheKey);
                    }
                }
                if (value.CurrentState == CacheValueState.Live && Clock.UtcNow() >= value.ExpirationTime)
                {
                    // Time to reload
                    lock (keyLockObj)
                    {
                        if (value.CurrentState == CacheValueState.Live)
                        {
                            value.CurrentState = CacheValueState.Refreshing;
                        }
                    }
                    Task.Factory.StartNew(dataProvider).ContinueWith(x =>
                    {
                        lock (keyLockObj)
                        {
                            value.Value = x.Result;
                            value.CurrentState = CacheValueState.Live;
                            value.ExpirationTime = GetRefreshTime(refreshIn);
                            MemoryCache.Default.Set(cacheKey, value, Clock.Now().Add(Settings.MaxTimeInCache));
                        }
                    });
                }
            }
            return value.Value;
        }

        private static object LockForKey(string key)
        {
            return keyLockDictionary.GetOrAdd(key, new object());
        }

        private static DateTime GetRefreshTime(TimeSpan refreshIn)
        {
            return Clock.UtcNow().Add(refreshIn);
        }

        private static CacheValue<T> GetCacheValueFor<T>(string key)
        {
            return MemoryCache.Default[key] as CacheValue<T>;
        }

        public static void Remove(string key)
        {
            lock (LockForKey(key)) {
                MemoryCache.Default.Remove(key);
            }
        }
    }
}
