using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    public class ILDbNetFunctionProvider : DbNetFunctionProvider
    {
        private readonly AssemblyBuilder ass_bulider;

        private readonly ModuleBuilder module_bulider;

        private readonly ConcurrentDictionary<Type, object> cache_imp = new ConcurrentDictionary<Type, object>();

        public ILDbNetFunctionProvider()
        {
            AssemblyName name = new AssemblyName("DbInterface.dll");
            ass_bulider = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            module_bulider = ass_bulider.DefineDynamicModule("DbInterface.dll");
        }

        public override TFunction GetFunction<TFunction>()
        {
            Type tType = typeof(TFunction);
            object obj = null;
            if (cache_imp.TryGetValue(tType, out obj))
            {
                //若之前已创建则取缓存的
                return (TFunction)obj;
            }
            else
            {
                throw new Exception("必须注册接口类型:" + tType.Name);
            }
        }

        public override void RegistFunction<TFunction>(DbNetConfiguration configuration)
        {
            Type function_type = typeof(TFunction);
            DbRouteAttribute routeAttribute = function_type.GetCustomAttribute<DbRouteAttribute>(true);
            Type dbroute_type = null;
            if (configuration.RouteCollection.ContainsKey("*"))
            {
                dbroute_type = configuration.RouteCollection["*"];
            }
            if (routeAttribute != null)
            {
                if (!configuration.RouteCollection.ContainsKey(routeAttribute.Name))
                {
                    throw new ArgumentException("不存在路由:" + routeAttribute.Name);
                }
                dbroute_type = configuration.RouteCollection[routeAttribute.Name];
            }
            var methods = function_type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in methods)
            {
                //路由类型
                Type m_route_type = dbroute_type;
                DbRouteAttribute m_route = m.GetCustomAttribute<DbRouteAttribute>(true);
                if (m_route != null)
                {
                    if (!configuration.RouteCollection.ContainsKey(m_route.Name))
                    {
                        throw new ArgumentException("不存在路由:" + m_route.Name);
                    }
                    m_route_type = configuration.RouteCollection[m_route.Name];
                }
                if (m_route_type == null)
                {
                    throw new Exception("未配置任何数据库路由");
                }
            }
            
        }

    }
}
