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
        private const string KEY_FORMAT = "{1}Imp_{0}";

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
            string route_name = string.Empty;
            Type function_type = typeof(TFunction);
            //获取接口类型中设定的路由
            DbRouteAttribute routeAttribute = function_type.GetCustomAttribute<DbRouteAttribute>(true);
            Type dbroute_type = null;
            if (configuration.RouteCollection.ContainsKey("*"))
            {
                //默认路由 关键字为*
                dbroute_type = configuration.RouteCollection["*"];
            }
            if (routeAttribute != null)
            {
                if (!configuration.RouteCollection.ContainsKey(routeAttribute.Name))
                {
                    throw new ArgumentException("不存在路由:" + routeAttribute.Name);
                }
                //若接口类型中设定的路由存在，则设置为该路由
                dbroute_type = configuration.RouteCollection[routeAttribute.Name];
                route_name = routeAttribute.Name;
            }
            var methods = function_type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            //创建实现类型
            TypeBuilder tb = module_bulider.DefineType(string.Format(KEY_FORMAT, function_type.Name, function_type.Namespace + "."), TypeAttributes.Class | TypeAttributes.Public, typeof(DbContext));
            tb.AddInterfaceImplementation(function_type);
            foreach (var m in methods)
            {
                //路由类型
                Type m_route_type = dbroute_type;
                DbRouteAttribute m_route = m.GetCustomAttribute<DbRouteAttribute>(true);
                DbFunctionAttribute m_fun = m.GetCustomAttribute<DbFunctionAttribute>(true);
                if (m_route != null)
                {
                    if (!configuration.RouteCollection.ContainsKey(m_route.Name))
                    {
                        throw new ArgumentException("不存在路由:" + m_route.Name);
                    }
                    //判断接口方法中是否存在路由，如果有则设定为该路由类型
                    m_route_type = configuration.RouteCollection[m_route.Name];
                }
                if (m_route_type == null)
                {
                    throw new ArgumentException(string.Format("方法：{0} 未设置任何路由", m.Name));
                }
                #region IL代码开始
                var paramterList = m.GetParameters();
                var tm = tb.DefineMethod(m.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.RTSpecialName, CallingConventions.HasThis,
                m.ReturnType, paramterList.Select(x => x.ParameterType).ToArray());
                //定义方法实现内容
                ILGenerator gen = tm.GetILGenerator();
                ExecuteType executeType = m_fun.ExecuteType;
                GetExecuteType(m, ref executeType);
                bool user_cache = m_fun.UserCache;
                bool user_tran = m_fun.UseTransaction;
                LocalBuilder result_builder = gen.DeclareLocal(m.ReturnType);//定义返回变量
                LocalBuilder sqlText_bulider = gen.DeclareLocal(typeof(string));//定义Sql语句变量
                LocalBuilder sqlConnection_bulider = gen.DeclareLocal(typeof(string));//定义数据库连接变量
                LocalBuilder hasCache_bulider = gen.DeclareLocal(typeof(bool));//定义是否存在缓存变量
                LocalBuilder dbNetProvider = gen.DeclareLocal(typeof(IDbNetProvider));//定义数据库提供程序变量
                LocalBuilder cacheProvider = gen.DeclareLocal(typeof(IDbNetCacheProvider));//定义缓存提供程序变量
                LocalBuilder exceptionBulider = gen.DeclareLocal(typeof(Exception));//定义异常变量，使用try catch
                LocalBuilder paramterBulider = gen.DeclareLocal(typeof(DbNetParamterCollection));//定义封装参数的集合变量
                gen.BeginExceptionBlock();//try开始
                if (user_cache)
                {
                    //若存在缓存，创建缓存提供程序
                    gen.Emit(OpCodes.Newobj, configuration.CacheProviderType.GetConstructor(Type.EmptyTypes));
                    gen.Emit(OpCodes.Stloc, cacheProvider);
                }

                //初始化参数集合
                gen.Emit(OpCodes.Newobj, typeof(DbNetParamterCollection).GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, paramterBulider);

                #region 获取输入参数

                GetParamters(paramterList, gen, paramterBulider);

                #endregion

                #region 创建路由获取数据库连接以及数据库提供程序

                if (configuration.DbProvider != null)
                {
                    //创建默认的提供程序
                    gen.Emit(OpCodes.Newobj, configuration.DbProvider.GetConstructor(Type.EmptyTypes));
                    gen.Emit(OpCodes.Stloc, dbNetProvider);
                }

                //执行路由中的方法RouteDbConnection
                LocalBuilder route_bulider = gen.DeclareLocal(m_route_type);
                gen.Emit(OpCodes.Newobj, m_route_type.GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, route_bulider);
                gen.Emit(OpCodes.Ldloc, route_bulider);
                gen.Emit(OpCodes.Ldstr, route_name);
                gen.Emit(OpCodes.Ldstr, function_type.Name);
                gen.Emit(OpCodes.Ldstr, m.Name);
                gen.Emit(OpCodes.Ldloc, paramterBulider);
                gen.Emit(OpCodes.Ldloca, dbNetProvider.LocalIndex);
                gen.Emit(OpCodes.Call, MethodHelper.dbNetRouteMethod);
                gen.Emit(OpCodes.Stloc, sqlConnection_bulider);
                #endregion
                gen.BeginCatchBlock(typeof(Exception));
                //catch处理
                gen.Emit(OpCodes.Stloc, exceptionBulider);
                gen.Emit(OpCodes.Ldloc, exceptionBulider);
                gen.Emit(OpCodes.Call, MethodHelper.exceptionMethod);
                gen.EndExceptionBlock();

                //若有out参数返回值在此处理
                //若存在缓存在此处理
                //若存在事务在此处理

                #region 返回结果
                if (m.ReturnType != null)
                {
                    gen.Emit(OpCodes.Ldloc, result_builder);
                }
                gen.Emit(OpCodes.Ret);
                #endregion

                #endregion
            }
            var t = tb.CreateType();
            cache_imp.TryAdd(function_type, Activator.CreateInstance(t));

        }

        /// <summary>
        /// 获取参数封装入集合
        /// </summary>
        /// <param name="paramterList"></param>
        /// <param name="gen"></param>
        /// <param name="paramterListBulider"></param>
        private static void GetParamters(ParameterInfo[] paramterList, ILGenerator gen,LocalBuilder paramterListBulider)
        {
            foreach (var paramter in paramterList)
            {
                Type pType = paramter.ParameterType;
                LocalBuilder pTypeBulider = null;
                DbNetParamterDirection dir = DbNetParamterDirection.Input;
                if (pType != typeof(IDbNetScope) &&
                    pType != typeof(IDbNetScope).MakeByRefType())
                {
                    if (!paramter.IsOut &&
                        !pType.IsByRef)
                    {
                        pTypeBulider = gen.DeclareLocal(pType);
                        gen.Emit(OpCodes.Ldarg, paramter.Position + 1);
                    }
                    else
                    {
                        dir = DbNetParamterDirection.InputAndOutPut;
                        pTypeBulider = gen.DeclareLocal(GetNormalType(pType));
                        if (pType.IsByRef)
                        {
                            //获取ref参数的值
                            gen.Emit(OpCodes.Ldarg, paramter.Position + 1);
                            GetRef(gen, pTypeBulider.LocalType);
                        }
                    }
                    gen.Emit(OpCodes.Stloc, pTypeBulider);
                    if (pTypeBulider.LocalType.IsClass)
                    {
                        Label isNullLabel = gen.DefineLabel();
                        gen.Emit(OpCodes.Ldloc, pTypeBulider);
                        gen.Emit(OpCodes.Ldnull);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Brfalse, isNullLabel);
                        gen.Emit(OpCodes.Call, MethodHelper.defaultMethod.MakeGenericMethod(pTypeBulider.LocalType));
                        gen.Emit(OpCodes.Stloc, pTypeBulider);
                        gen.MarkLabel(isNullLabel);
                    }
                    if (paramter.IsOut)
                    {
                        dir = DbNetParamterDirection.Output;
                        //初始化赋值输出参数
                        gen.Emit(OpCodes.Call, MethodHelper.defaultMethod.MakeGenericMethod(pTypeBulider.LocalType));
                        gen.Emit(OpCodes.Stloc, pTypeBulider);
                        gen.Emit(OpCodes.Ldarg, paramter.Position + 1);
                        gen.Emit(OpCodes.Ldloc, pTypeBulider);
                        SetRef(gen, pTypeBulider.LocalType);
                    }
                }
                if (pTypeBulider.LocalType == typeof(string) || 
                    pTypeBulider.LocalType.IsValueType||
                    pTypeBulider.LocalType.IsArray)
                {
                    //添加结构体或者string到集合
                    gen.Emit(OpCodes.Ldloc, paramterListBulider);
                    gen.Emit(OpCodes.Ldstr, paramter.Name);
                    gen.Emit(OpCodes.Ldloc, pTypeBulider);
                    gen.Emit(OpCodes.Ldc_I4, (int)dir);
                    gen.Emit(OpCodes.Ldc_I4, paramter.Position);
                    gen.Emit(OpCodes.Ldc_I4, (int)SourceType.FromArg);
                    gen.Emit(OpCodes.Call, MethodHelper.paramterMethod.MakeGenericMethod(pTypeBulider.LocalType));
                }
                else
                {
                    //如果参数是类则需进一步处理
                    var pinfo = pTypeBulider.LocalType.GetProperties();
                    foreach (var p in pinfo)
                    {
                        DbParamterAttribute attr_p = p.GetCustomAttribute<DbParamterAttribute>(true);
                        bool except = false;
                        string name = string.Empty;
                        DbNetParamterDirection pdir = DbNetParamterDirection.Input;
                        if (paramter.IsOut)
                        {
                            pdir = DbNetParamterDirection.Output;
                        }
                        else if (pType.IsByRef)
                        {
                            pdir = DbNetParamterDirection.InputAndOutPut;
                        }
                        if (attr_p != null)
                        {
                            except = attr_p.Except;
                            name = attr_p.Name;
                            pdir = attr_p.ParameterDirection;
                        }
                        if (string.IsNullOrEmpty(name))
                        {
                            name = p.Name;
                        }
                        if (!except)
                        {
                            gen.Emit(OpCodes.Ldloc, paramterListBulider);
                            gen.Emit(OpCodes.Ldstr, name);
                            gen.Emit(OpCodes.Ldloc, pTypeBulider);
                            gen.Emit(OpCodes.Call, p.GetGetMethod());
                            gen.Emit(OpCodes.Ldc_I4, (int)pdir);
                            gen.Emit(OpCodes.Ldc_I4, paramter.Position);
                            gen.Emit(OpCodes.Ldc_I4, (int)SourceType.FromClass);
                            gen.Emit(OpCodes.Call, MethodHelper.paramterMethod.MakeGenericMethod(p.PropertyType));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取非引用类型
        /// </summary>
        /// <param name="pType"></param>
        /// <returns></returns>
        private static Type GetNormalType(Type pType)
        {
            Type mType = pType;
            if (mType.FullName.EndsWith("&"))
            {
                mType = pType.Assembly.GetType(pType.FullName.TrimEnd('&'));
            }

            return mType;
        }

        /// <summary>
        /// 获取需要执行的命令类型
        /// </summary>
        /// <param name="m"></param>
        /// <param name="executeType"></param>
        private void GetExecuteType(MethodInfo m, ref ExecuteType executeType)
        {
            if (executeType != ExecuteType.Default)
            {
                //执行命令类型的默认设置
                if (m.ReturnType != null)
                {
                    if (m.ReturnType == typeof(int))
                    {
                        executeType = ExecuteType.ExecuteNoQuery;
                    }
                    else if (m.ReturnType.IsValueType || m.ReturnType == typeof(string))
                    {
                        executeType = ExecuteType.ExecuteObject;
                    }
                    else
                    {
                        executeType = ExecuteType.ExecuteDateTable;
                    }
                }
                else
                {
                    executeType = ExecuteType.ExecuteNoQuery;
                }
            }
        }

        /// <summary>
        /// IL引用类型使用
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="type"></param>
        private static void GetRef(ILGenerator gen, Type type)
        {
            if (type == typeof(string))
            {
                gen.Emit(OpCodes.Ldind_Ref);
            }
            else if (type == typeof(DateTime))
            {
                gen.Emit(OpCodes.Ldobj, type);
            }
            else if (type == typeof(byte))
            {
                gen.Emit(OpCodes.Ldind_I1);
            }
            else if (type == typeof(bool))
            {
                gen.Emit(OpCodes.Ldind_I1);
            }
            else if (type == typeof(Guid))
            {
                gen.Emit(OpCodes.Ldobj, type);
            }
            else if (type == typeof(decimal))
            {
                gen.Emit(OpCodes.Ldobj, type);
            }
            else if (type == typeof(short))
            {
                gen.Emit(OpCodes.Ldind_I2);
            }
            else if (type == typeof(int))
            {
                gen.Emit(OpCodes.Ldind_I4);
            }
            else if (type == typeof(long))
            {
                gen.Emit(OpCodes.Ldind_I8);
            }
            else if (type == typeof(double))
            {
                gen.Emit(OpCodes.Ldind_R8);
            }
            else if (type == typeof(float))
            {
                gen.Emit(OpCodes.Ldind_R4);
            }
            else if (type == typeof(sbyte))
            {
                gen.Emit(OpCodes.Ldind_U1);
            }
            else if (type == typeof(ushort))
            {
                gen.Emit(OpCodes.Ldind_U2);
            }
            else if (type == typeof(uint))
            {
                gen.Emit(OpCodes.Ldind_U4);
            }
            else if (type == typeof(ulong))
            {
                gen.Emit(OpCodes.Ldind_I8);
            }
            else
            {
                gen.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// IL引用类型赋值
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="type"></param>
        private static void SetRef(ILGenerator gen, Type type)
        {
            if (type == typeof(string))
            {
                gen.Emit(OpCodes.Stind_Ref);
            }
            else if (type == typeof(DateTime))
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else if (type == typeof(byte))
            {
                gen.Emit(OpCodes.Stind_I1);
            }
            else if (type == typeof(bool))
            {
                gen.Emit(OpCodes.Stind_I1);
            }
            else if (type == typeof(Guid))
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else if (type == typeof(decimal))
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else if (type == typeof(short))
            {
                gen.Emit(OpCodes.Stind_I2);
            }
            else if (type == typeof(int))
            {
                gen.Emit(OpCodes.Stind_I4);
            }
            else if (type == typeof(long))
            {
                gen.Emit(OpCodes.Stind_I8);
            }
            else if (type == typeof(double))
            {
                gen.Emit(OpCodes.Stind_R8);
            }
            else if (type == typeof(float))
            {
                gen.Emit(OpCodes.Stind_R4);
            }
            else if (type == typeof(sbyte))
            {
                gen.Emit(OpCodes.Stind_I1);
            }
            else if (type == typeof(ushort))
            {
                gen.Emit(OpCodes.Stind_I2);
            }
            else if (type == typeof(uint))
            {
                gen.Emit(OpCodes.Stind_I4);
            }
            else if (type == typeof(ulong))
            {
                gen.Emit(OpCodes.Stind_I8);
            }
            else
            {
                gen.Emit(OpCodes.Stind_Ref);
            }
        }

    }
}
