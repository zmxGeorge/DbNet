using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace DbNet
{
    public class DbNetRouteCollection : ConcurrentDictionary<string, Type>
    {
    }

    /// <summary>
    /// DbNet配置
    /// </summary>
    public class DbNetConfiguration
    {
        private static readonly DbNetConfiguration _configuration = new DbNetConfiguration();

        private Type _dbProviderType = null;

        private Type _cacheProviderType = null;

        private DbNetFunctionProvider _dbNetFunctionProvider = null;

        private DbNetRouteCollection _route_map = new DbNetRouteCollection();

        /// <summary>
        /// 缓存提供程序类型
        /// </summary>
        public Type CacheProviderType { get { return _cacheProviderType; } }

        /// <summary>
        /// 数据库提供程序类型
        /// </summary>
        public Type DbProvider { get { return _dbProviderType; } }

        /// <summary>
        /// 数据库路由以及其类型
        /// </summary>
        public DbNetRouteCollection RouteCollection { get { return _route_map; } }

        /// <summary>
        /// 添加默认数据库提供程序
        /// </summary>
        /// <typeparam name="TDbProvider"></typeparam>
        public static void AddDbProvider<TDbProvider>()
            where TDbProvider : IDbNetProvider
        {
            _configuration._dbProviderType = typeof(TDbProvider);
        }

        /// <summary>
        /// 添加默认接口缓存提供程序
        /// </summary>
        /// <typeparam name="TCacheProvider"></typeparam>
        public static void AddCacheProvider<TCacheProvider>()
            where TCacheProvider : IDbNetCacheProvider
        {
            _configuration._cacheProviderType = typeof(TCacheProvider);
        }

        /// <summary>
        /// 添加接口注入提供程序
        /// </summary>
        /// <typeparam name="TFunctionProvider"></typeparam>
        /// <param name="provider"></param>
        public static void AddFunctionProvider<TFunctionProvider>()
            where TFunctionProvider : DbNetFunctionProvider,new()
        {
            _configuration._dbNetFunctionProvider = new TFunctionProvider();
        }

        /// <summary>
        /// 注册接口
        /// </summary>
        /// <typeparam name="TFunction"></typeparam>
        public static void RegistFunction<TFunction>() where TFunction : IDbFunction
        {
            if (_configuration._dbNetFunctionProvider == null)
            {
                throw new Exception("未设置接口注入提供程序");
            }
            _configuration._dbNetFunctionProvider.RegistFunction<TFunction>(_configuration);
        }

        /// <summary>
        /// 获取接口实例
        /// </summary>
        /// <typeparam name="TFunction"></typeparam>
        /// <returns></returns>
        public static TFunction GetFunction<TFunction>() where TFunction : IDbFunction
        {
            if (_configuration._dbNetFunctionProvider == null)
            {
                throw new Exception("未设置接口注入提供程序");
            }
            return _configuration._dbNetFunctionProvider.GetFunction<TFunction>();
        }

        /// <summary>
        /// 映射数据库路由
        /// </summary>
        /// <typeparam name="TDbRouteProvider">数据库路由</typeparam>
        /// <typeparam name="TDbProvider">对应的数据库提供程序</typeparam>
        /// <param name="routeName">路由名称</param>
        public static void MapRoute<TDbRouteProvider>(string routeName)
            where TDbRouteProvider : IDbNetRouteProvider,new()
        {
            if (_configuration._route_map.ContainsKey(routeName)&&
                routeName!="*")
            {
                throw new ArgumentException("路由名称不能重复");
            }
            if (_configuration._route_map.ContainsKey(routeName) &&
                routeName == "*")
            {
                _configuration._route_map[routeName] = typeof(TDbRouteProvider);
            }
            else
            {
                _configuration._route_map.TryAdd(routeName, typeof(TDbRouteProvider));
            }
        }
     }
}
