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
    public interface IDbNetScope:IDisposable
    {
        /// <summary>
        /// 开始一个范围
        /// </summary>
        void Open();

        /// <summary>
        /// 每当执行一次接口时操作
        /// </summary>
        void Commit();

        /// <summary>
        /// 每当执行一次接口时回滚操作
        /// </summary>
       void Rollback();

        /// <summary>
        /// 每当执行一次接口时会关闭
        /// </summary>
        void Close();
    }
}
