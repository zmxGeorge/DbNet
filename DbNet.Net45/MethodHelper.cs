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

        public static readonly MethodInfo getValueMethod = typeof(DbNetParamterCollection).GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo defaultMethod = typeof(MethodHelper).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.Public);

        public static readonly MethodInfo exceptionMethod = typeof(DbContext).GetMethod("RaiseEvent",BindingFlags.Static|BindingFlags.Public);

        public static readonly MethodInfo cache_getKeyMethod = typeof(IDbNetCacheProvider).GetMethod("GetKey", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo cache_getCacheMethod = typeof(IDbNetCacheProvider).GetMethod("GetCache", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo cache_addCacheMethod = typeof(IDbNetCacheProvider).GetMethod("AddCache", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo cacheItem_getParamters = typeof(SQLCacheItem).GetMethod("GetParamters", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo cacheItem_getResult = typeof(SQLCacheItem).GetMethod("GetReult", BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo db_exec_Method = typeof(IDbNetProvider).GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.Public);

        public static readonly ConstructorInfo command_bulid_Method = typeof(DbNetCommand).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(DbNetParamterCollection),
        typeof(string),typeof(DbNetIsolationLevel)});

        public static readonly FieldInfo none_level = typeof(DbNetIsolationLevel).GetField("None", BindingFlags.Public | BindingFlags.Static);

        public static readonly ConstructorInfo level_bulid = typeof(DbNetIsolationLevel).GetConstructor(new Type[] { typeof(string)});

        public static readonly MethodInfo cache_item_bulid = typeof(SQLCacheItem).GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public);


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
