using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace DbNet
{
    public class MemoryCacheProvider : DbNetCacheProvider
    {
        private static readonly System.Runtime.Caching.MemoryCache _memoryCache = new System.Runtime.Caching.MemoryCache("db_cache");

        private static readonly ConcurrentDictionary<string, SQLCacheItem> m_cache = new ConcurrentDictionary<string, SQLCacheItem>();

        public override void AddCache(string cacheKey, SQLCacheItem cacheItem, int cacheTime, int duringTime)
        {
            if (cacheTime == -1)
            {
                m_cache.TryAdd(cacheKey, cacheItem);
            }
            else
            {
                if (cacheTime > 0)
                {
                    _memoryCache.Add(new CacheItem(cacheKey) { Key = cacheKey, Value = cacheKey }, new CacheItemPolicy { AbsoluteExpiration=new DateTimeOffset(DateTime.Now,new TimeSpan(0,0,cacheTime)) });
                }
                else if (duringTime > 0)
                {
                    _memoryCache.Add(new CacheItem(cacheKey) { Key = cacheKey, Value = cacheKey }, new CacheItemPolicy { SlidingExpiration=new TimeSpan(0,0,duringTime) });
                }
            }
        }

        public override SQLCacheItem GetCache(string cacheKey, out bool hasCache)
        {
            if (m_cache.ContainsKey(cacheKey))
            {
                hasCache = true;
                return m_cache[cacheKey];
            }
            else
            {
                if (_memoryCache.Contains(cacheKey))
                {
                    hasCache = true;
                    return _memoryCache.Get(cacheKey) as SQLCacheItem;
                }
                else
                {
                    hasCache = false;
                    return null;
                }
            }
        }
    }
}
