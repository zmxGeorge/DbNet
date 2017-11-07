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
    /// 数据库支持提供程序接口
    /// </summary>
    public interface IDbNetProvider
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="command">命令</param>
        /// <param name="scope">执行范围</param>
        /// <param name="executeType">执行类型</param>
        /// <returns></returns>
        DbNetResult ExecuteCommand(DbNetCommand command, IDbNetScope scope, ExecuteType executeType);


    }
}
