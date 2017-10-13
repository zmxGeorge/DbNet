using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{
    /// <summary>
    /// 数据库路由提供程序
    /// 用于分布式处理
    /// </summary>
    public abstract class DbNetRouteProvider
    {
        /// <summary>
        /// 依据传入的关键字，指向不同的数据库或数据库连接
        /// </summary>
        /// <param name="routeName">路由名称</param>
        /// <param name="moduleName">接口名称</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="paramters">路由参数值集合</param>
        /// <param name="dbProvider">返回数据库提供程序</param>
        /// <returns>返回数据库连接，若不存在返回null或者空字符串将选择默认的数据库连接</returns>
        public abstract string RouteDbConnection(string routeName,string moduleName,string methodName,Dictionary<string,object> paramters,ref DbNetProvider dbProvider);
    }
}
