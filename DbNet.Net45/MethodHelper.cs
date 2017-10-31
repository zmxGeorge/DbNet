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
        public static readonly MethodInfo dbNetRouteMethod = typeof(DbNetRouteProvider).GetMethod("RouteDbConnection", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo paramterMethod = typeof(DbNetParamterCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo defaultMethod = typeof(MethodHelper).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.Public);

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
            return Activator.CreateInstance<T>();
        }
    }
}
