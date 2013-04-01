using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCache
{
    public class Cacher
    {
        private static ConcurrentDictionary<string, object> keyLockDictionary = new ConcurrentDictionary<string, object>();

        private static object LockForKey(string key) {
            return keyLockDictionary.GetOrAdd(key, new object());
        }

        public static T Get<T>(string cacheKey, Func<T> dataProvider, int cacheTimeInMinutes = 10)
        {
            object keyLockObj = LockForKey(cacheKey);

            CacheValue<T> value = GetCacheValueFor<T>(cacheKey);
            if (value == null)
            {
                lock (keyLockObj) {
                    value = GetCacheValueFor<T>(cacheKey);
                    if (value == null)
                    {
                        value = new CacheValue<T>()
                        {
                            CurrentState = CacheValueState.Loading,
                            ExpirationTime = GetExpirationTime(cacheTimeInMinutes)
                        };
                        MemoryCache.Default.Add(cacheKey, value, DateTime.Now.AddMinutes(cacheTimeInMinutes * 2));

                        value.Value = dataProvider();
                        value.CurrentState = CacheValueState.Live;
                        value.ExpirationTime = GetExpirationTime(cacheTimeInMinutes);
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
                        if (value.CurrentState == CacheValueState.Live){
                            value.CurrentState = CacheValueState.Refreshing;
                        }
                    }
                    Task.Factory.StartNew(() => value.Value = dataProvider()).ContinueWith(x => 
                    {
                        value.CurrentState = CacheValueState.Live;
                        value.ExpirationTime = GetExpirationTime(cacheTimeInMinutes);
                        MemoryCache.Default.Set(cacheKey, value, DateTime.Now.AddMinutes(cacheTimeInMinutes * 2));
                    });
                }
            }
            return value.Value;
        }


        private static DateTime GetExpirationTime(int lifeTimeMinutes)
        {
            return Clock.UtcNow().AddMinutes(lifeTimeMinutes);
        }

        private static CacheValue<T> GetCacheValueFor<T>(string key)
        {
            return MemoryCache.Default[key] as CacheValue<T>;
        }
    }

    class CacheValue<T>
    {
        private CacheValueState currentState = CacheValueState.Loading;

        public T Value { get; set; }
        public DateTime ExpirationTime { get; set; }
        public CacheValueState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
    }

    enum CacheValueState
    {
        Live,
        Loading,
        Refreshing
    }
}
