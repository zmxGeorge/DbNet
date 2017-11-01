using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace DbNet
{
    /// <summary>
    /// 缓存关键字类型
    /// </summary>
    public enum CacheKeyType
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default=-1,
        /// <summary>
        /// 不作为缓存关键字处理
        /// </summary>
        None=0,
        /// <summary>
        /// 作为缓存关键字处理
        /// </summary>
        Bind=1
    }

    /// <summary>
    /// 参数配置项
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbParamterAttribute:Attribute
    {
        public DbParamterAttribute()
        {
            CacheKey = CacheKeyType.Default;
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数方向
        /// </summary>
        public DbNetParamterDirection ParameterDirection { get; set; }

        /// <summary>
        /// 是否排除改映射项
        /// </summary>
        public bool Except { get; set; }

        /// <summary>
        /// 是否作为缓存关键字
        /// </summary>
        public CacheKeyType CacheKey { get; set; }
    }
}
