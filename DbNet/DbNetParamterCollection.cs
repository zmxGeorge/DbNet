using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 参数源
    /// </summary>
    public enum SourceType
    {
        /// <summary>
        /// 来自方法基本参数
        /// </summary>
        FromArg=0,
        /// <summary>
        /// 来自方法基本参数包含的强类型
        /// </summary>
        FromClass=1
    }

    /// <summary>
    /// 参数集合
    /// </summary>
    public class DbNetParamterCollection:IEnumerable<DbNetParamter>
    {
        private readonly Dictionary<string, DbNetParamter> dic = new Dictionary<string, DbNetParamter>();

        public DbNetParamterCollection()
        {
        }

        public void Add<T>(string key, T value, DbNetParamterDirection direction,int sourceIndex,SourceType sourceType,CacheKeyType cacheKeyType)
        {
            if (dic.ContainsKey(key))
            {
                throw new Exception(string.Format("参数名称重复，来自 参数位置:{0} 源:{1}",sourceIndex,sourceType==SourceType.FromClass?"来自类属性": 
                    sourceType == SourceType.FromArg ? "来自方法参数":"未知"));
            }
            dic.Add(key,new DbNetParamter { Name = key, Value = value, Direction = direction,SourceIndex=sourceIndex,SourceType= sourceType,CacheKeyType= cacheKeyType });
        }

        public DbNetParamter Get(string key)
        {
            if (!dic.ContainsKey(key))
            {
                return null;
            }
            else
            {
                return dic[key];
            }
        }

        public IEnumerator<DbNetParamter> GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }

    }
}
