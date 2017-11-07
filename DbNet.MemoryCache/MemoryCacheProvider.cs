using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DbNet
{
    /// <summary>
    /// 存入内存的缓存提供程序
    /// </summary>
    public class MemoryCacheProvider : IDbNetCacheProvider
    {
        private const string X_TWO = "X2";

        private const string KEY_FROMAT = "{0}.{1}----- {2}";

        private static readonly System.Runtime.Caching.MemoryCache _memoryCache = new System.Runtime.Caching.MemoryCache("db_cache");

        private static readonly ConcurrentDictionary<string, ISQLCacheItem> m_cache = new ConcurrentDictionary<string, ISQLCacheItem>();

        /// <summary>
        /// 生成缓存键
        /// 依据哈希字符串原来
        /// 将参数转换为字节数组，然后做SHA256处理
        /// </summary>
        /// <param name="paramterCollection"></param>
        /// <returns></returns>
        public string GetKey(string functionName,string methodName,string sqlText,DbNetParamterCollection paramterCollection)
        {
            Dictionary<string, object> paramterValues = new Dictionary<string, object>();
            foreach (var item in paramterCollection)
            {
                if (item.CacheKeyType == CacheKeyType.Bind)
                {
                    paramterValues.Add(item.Name, item.Value);
                }
            }
            //二进制序列化成字节数组
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            byte[] data = null;
            using (MemoryStream mstream = new MemoryStream())
            {
                binaryFormatter.Serialize(mstream, paramterValues);
                data = mstream.ToArray();
            }
            //哈希处理
            SHA256 sha = new SHA256CryptoServiceProvider();
            data = sha.ComputeHash(data);
            data = Encoding.UTF8.GetBytes(string.Format(functionName, methodName, sqlText)).Concat(data).ToArray();
            data = sha.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append(b.ToString(X_TWO));
            }
            return sb.ToString();
        }

        public void AddCache(string cacheKey, ISQLCacheItem cacheItem, int cacheTime, int duringTime)
        {
            if (cacheTime == -1)
            {
                m_cache.TryAdd(cacheKey, cacheItem);
            }
            else
            {
                if (cacheTime > 0)
                {
                    _memoryCache.Add(new CacheItem(cacheKey) { Key = cacheKey, Value = cacheItem }, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.Now, new TimeSpan(0, 0, cacheTime)) });
                }
                else if (duringTime > 0)
                {
                    _memoryCache.Add(new CacheItem(cacheKey) { Key = cacheKey, Value = cacheItem }, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, duringTime) });
                }
            }
        }

        public ISQLCacheItem GetCache(string cacheKey, out bool hasCache)
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
                    return _memoryCache.Get(cacheKey) as ISQLCacheItem;
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
