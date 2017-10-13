using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet.Net45
{
    /// <summary>
    /// 执行异常委托
    /// </summary>
    /// <param name="e"></param>
    public delegate void ExecuteException(Exception e);

    /// <summary>
    /// 接口实例被创建委托
    /// </summary>
    /// <param name="function">接口实例</param>
    public delegate void FunctionInit(IDbFunction function);

    /// <summary>
    /// 执行接口的委托
    /// </summary>
    /// <param name="function">接口实例</param>
    /// <param name="methodName">执行方法的名称</param>
    public delegate void ExecuteMethodHandler(IDbFunction function, string methodName);

    /// <summary>
    /// 所有注入类都继承该类
    /// </summary>
    public abstract class DbContext
    {
        /// <summary>
        /// 数据接口执行异常处理事件
        /// </summary>
        public static event ExecuteException SqlException;

        /// <summary>
        /// 数据接口初始化事件
        /// </summary>
        public static event FunctionInit FunctionInit;

        /// <summary>
        /// 开始执行接口事件
        /// </summary>
        public static event ExecuteMethodHandler ExecuteStart;

        /// <summary>
        /// 接口执行结束事件
        /// </summary>
        public static event ExecuteMethodHandler ExecuteEnd;
    }
}
