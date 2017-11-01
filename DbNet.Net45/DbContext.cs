using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 执行异常委托
    /// </summary>
    /// <param name="e"></param>
    public delegate void ExecuteException(Exception e);

    /// <summary>
    /// 所有注入类都继承该类
    /// </summary>
    public abstract class DbContext
    {
        /// <summary>
        /// 数据接口执行异常处理事件
        /// </summary>
        public static event ExecuteException SqlException;

        public static void RaiseEvent(Exception e)
        {
            if (SqlException != null)
            {
                SqlException(e);
            }
        }

    }
}
