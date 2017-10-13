using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet.Net45
{
    /// <summary>
    /// 所有注入类都继承该类
    /// </summary>
    public abstract class DbContext
    {
        /// <summary>
        /// 数据接口执行异常处理事件
        /// </summary>
        public static event Action<Exception> SqlException;

        /// <summary>
        /// 数据接口初始化事件
        /// </summary>
        public static event Action<IDbFunction> FunctionInit;

        /// <summary>
        /// 开始执行接口事件
        /// </summary>
        public static event Action<IDbFunction, string> ExecuteStart;

        /// <summary>
        /// 接口执行结束事件
        /// </summary>
        public static event Action<IDbFunction, string> ExecuteEnd;
    }
}
