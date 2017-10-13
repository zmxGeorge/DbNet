using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 接口注入提供程序
    /// </summary>
    public abstract class DbNetFunctionProvider
    {
        /// <summary>
        /// 注册接口
        /// </summary>
        /// <typeparam name="TFunction">接口类型</typeparam>
        /// <param name="configuration">配置项</param>
        public abstract void RegistFunction<TFunction>(DbNetConfiguration configuration) where TFunction:IDbFunction;

        /// <summary>
        /// 获取接口
        /// </summary>
        /// <typeparam name="TFunction">接口类型</typeparam>
        /// <returns>接口实例</returns>
        public abstract TFunction GetFunction<TFunction>() where TFunction : IDbFunction;
    }
}
