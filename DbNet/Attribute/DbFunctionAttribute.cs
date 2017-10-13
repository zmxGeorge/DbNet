using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DbNet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DbFunctionAttribute:Attribute
    {
        public DbFunctionAttribute()
        {
            CommandType = string.Empty;
            IsolationLevel = DbNetIsolationLevel.None;
            UseTransaction = false;
            CacheTime = 0;
        }

        /// <summary>
        /// 存储过程名称或者Sql语句名称
        /// </summary>
        public string SqlText { get; set; }

        /// <summary>
        /// 包含Sql语句或存储过程名称参数的名称
        /// 若Sql语句是动态Sql语句，设置该值
        /// </summary>
        public string SqlTextKey { get; set; }

        /// <summary>
        /// 命令类型
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// 是否使用事务
        /// </summary>
        public bool UseTransaction { get; set; }

        /// <summary>
        /// 若使用事务设置此参数
        /// </summary>
        public DbNetIsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// 缓存关键字
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool UserCache { get; set; }

        /// <summary>
        /// 设置最大缓存时间
        /// 在满足DuringTime的条件下
        /// 超过该时间间隔缓存过期
        /// -1代表永久缓存
        /// 若CacheTime为-1 DuringTime将不起作用
        /// 设置单位:秒
        /// </summary>
        public int CacheTime { get; set; }

        /// <summary>
        /// 设置缓存时间间隔
        /// 在缓存未被访问多长时间情况下缓存过期
        /// 设置单位:秒
        /// </summary>
        public int DuringTime { get; set; }

    }
}
