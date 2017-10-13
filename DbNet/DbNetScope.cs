using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 表示一个数据库执行范围
    /// </summary>
    public abstract class DbNetScope:IDisposable
    {
        /// <summary>
        /// 开始一个范围
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// 每当执行一次接口时操作
        /// </summary>
        public abstract void Commit();

        /// <summary>
        /// 每当执行一次接口时回滚操作
        /// </summary>
        public abstract void Rollback();

        /// <summary>
        /// 每当执行一次接口时会关闭
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 释放资源的操作
        /// </summary>
        public abstract void Dispose();
    }
}
