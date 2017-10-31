using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace DbNet
{
    /// <summary>
    /// 数据库执行类型
    /// </summary>
    public enum ExecuteType
    {
        /// <summary>
        /// 默认设置
        /// </summary>
        Default=0,
        /// <summary>
        /// 不做查询，执行数据库命令，返回受影响行数
        /// </summary>
        ExecuteNoQuery=1,
        /// <summary>
        /// 查询第一行第一个的值
        /// </summary>
        ExecuteObject=2,
        /// <summary>
        /// 查询结果集
        /// </summary>
        ExecuteDateTable=3
    }

    /// <summary>
    /// 数据库支持提供程序
    /// </summary>
    public abstract class DbNetProvider
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="command">命令</param>
        /// <param name="scope">执行范围</param>
        /// <param name="executeType">执行类型</param>
        /// <returns></returns>
        public abstract object ExecuteCommand<TResult>(DbNetCommand command, DbNetScope scope, ExecuteType executeType);

        /// <summary>
        /// 获取一个可用的执行范围
        /// </summary>
        /// <param name="scope">可能由外部传入的值</param>
        /// <param name="command">执行命令</param>
        /// <returns></returns>
        public abstract DbNetScope GetScope(DbNetScope scope, DbNetCommand command);

    }
}
