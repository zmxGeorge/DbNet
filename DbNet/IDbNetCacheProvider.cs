using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{

    /// <summary>
    /// 缓存提供程序
    /// </summary>
    public interface IDbNetCacheProvider
    {
        /// <summary>
        /// 生成缓存键
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="methodName"></param>
        /// <param name="sqlText"></param>
        /// <param name="paramterCollection"></param>
        /// <returns></returns>
        string GetKey(string functionName,string methodName,string sqlText,DbNetParamterCollection paramterCollection);

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="hasCache"></param>
        /// <returns></returns>
        SQLCacheItem GetCache(string cacheKey, out bool hasCache);

        /// <summary>
        /// 添加缓存对象
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheItem"></param>
        /// <param name="cacheTime"></param>
        /// <param name="duringTime"></param>
        void AddCache(string cacheKey, SQLCacheItem cacheItem, int cacheTime, int duringTime);
    }
}
