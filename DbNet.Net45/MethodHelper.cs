using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;

namespace DbNet
{
    public static class MethodHelper
    {
        public static readonly MethodInfo dbNetRouteMethod = typeof(IDbNetRouteProvider).GetMethod("RouteDbConnection", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo paramterMethod = typeof(DbNetParamterCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo defaultMethod = typeof(MethodHelper).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.Public);

        public static readonly MethodInfo exceptionMethod = typeof(DbContext).GetMethod("RaiseEvent", BindingFlags.Static | BindingFlags.Public);

        public static T ParseResult<T>(object obj) where T : new()
        {
            return default(T);
        }

        /// <summary>
        /// 默认值设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDefaultValue<T>()
        {
            if (typeof(T).IsArray)
            {
                return default(T);
            }
            else
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}
