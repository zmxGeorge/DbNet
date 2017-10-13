using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{
    /// <summary>
    /// 缓存提供程序
    /// </summary>
    public abstract class DbNetCacheProvider
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="hasCache"></param>
        /// <returns></returns>
        public abstract SQLCacheItem GetCache(string cacheKey, out bool hasCache);

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheItem"></param>
        /// <param name="cacheTime"></param>
        /// <param name="duringTime"></param>
        public abstract void AddCache(string cacheKey, SQLCacheItem cacheItem, int cacheTime, int duringTime);
    }
}
