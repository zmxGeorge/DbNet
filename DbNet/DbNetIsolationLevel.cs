using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 指定事务锁级别
    /// </summary>
    public class DbNetIsolationLevel
    {
        /// <summary>
        /// 默认事务锁级别
        /// </summary>
        public static readonly DbNetIsolationLevel None = new DefaultIsolationLevel();

        public string Level { get; set; }

        class DefaultIsolationLevel : DbNetIsolationLevel
        {
        }
        protected DbNetIsolationLevel()
        {
        }

        public DbNetIsolationLevel(string level)
        {
            Level = level;
        }
    }
}
