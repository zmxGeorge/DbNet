using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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

        private const string SCOPE_ITEM = "DbNet.IDbNetScope";

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
                if (paramterList.Count(x => (x.ParameterType.GetInterface(SCOPE_ITEM) != null ||
                   (x.ParameterType.GetElementType() != null && x.ParameterType.GetElementType().GetInterface(SCOPE_ITEM) != null))) > 1)
                {
                    throw new Exception("参数中实现IDbNetScope接口类型的参数不能超过一个");
                }
                //定义方法实现内容
                ILGenerator gen = tm.GetILGenerator();
                ExecuteType executeType = m_fun.ExecuteType;
                GetExecuteType(m, ref executeType);
                bool user_cache = m_fun.UserCache;
                bool user_tran = m_fun.UseTransaction;
                LocalBuilder result_builder = gen.DeclareLocal(m.ReturnType);//定义返回变量
                LocalBuilder sqlText_bulider = gen.DeclareLocal(typeof(string));//定义Sql语句变量
                LocalBuilder sqlConnection_bulider = gen.DeclareLocal(typeof(string));//定义数据库连接变量
                LocalBuilder cacheKey_bulider = gen.DeclareLocal(typeof(string));//定义缓存键变量
                LocalBuilder cacheItem_bulider = gen.DeclareLocal(typeof(SQLCacheItem));//定义缓存对象变量
                LocalBuilder netScope_bulider = gen.DeclareLocal(typeof(IDbNetScope));//定义NetScope
                LocalBuilder hasCache_bulider = gen.DeclareLocal(typeof(bool));//定义是否存在缓存变量
                LocalBuilder dbNetProvider = gen.DeclareLocal(typeof(IDbNetProvider));//定义数据库提供程序变量
                LocalBuilder cacheProvider = gen.DeclareLocal(typeof(IDbNetCacheProvider));//定义缓存提供程序变量
                LocalBuilder exceptionBulider = gen.DeclareLocal(typeof(Exception));//定义异常变量，使用try catch
                LocalBuilder paramterBulider = gen.DeclareLocal(typeof(DbNetParamterCollection));//定义封装参数的集合变量
                LocalBuilder hasCacheBulider = gen.DeclareLocal(typeof(bool));//定义是否存在缓存的变量
                LocalBuilder commandBulider = gen.DeclareLocal(typeof(DbNetCommand));//定义是否存在缓存的变量

                Label end_label = gen.DefineLabel();//指向执行结束的标签

                gen.BeginExceptionBlock();//try开始

                //初始化定义缓存是否存在的变量
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, hasCacheBulider);

                if (user_cache)
                {
                    //若存在缓存，创建缓存提供程序
                    gen.Emit(OpCodes.Newobj, configuration.CacheProviderType.GetConstructor(Type.EmptyTypes));
                    gen.Emit(OpCodes.Stloc, cacheProvider);
                }

                //初始化参数集合
                gen.Emit(OpCodes.Newobj, typeof(DbNetParamterCollection).GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, paramterBulider);
                ParameterInfo scopeParemterInfo = null;
                string sqlTextKey = null;
                if (m_fun != null)
                {
                    sqlTextKey = m_fun.SqlTextKey;
                }
                List<Tuple<string, int, MethodInfo, Type>> outputParamters = new List<Tuple<string, int, MethodInfo, Type>>();
                GetParamters(outputParamters, paramterList, gen, paramterBulider, user_cache, sqlTextKey, out scopeParemterInfo);

                //sql语句处理
                string sqlText = m.Name;
                if (m_fun != null && !string.IsNullOrEmpty(m_fun.SqlText))
                {
                    sqlText = m_fun.SqlText;
                    gen.Emit(OpCodes.Ldstr, sqlText);
                }
                else if (m_fun != null && !string.IsNullOrEmpty(sqlTextKey))
                {
                    var paramter = paramterList.FirstOrDefault(x => x.Name == sqlTextKey);
                    if (paramter == null)
                    {
                        throw new Exception("未能找到指定sql语句的参数，名称:" + sqlTextKey);
                    }
                    if (paramter.ParameterType != typeof(string))
                    {
                        throw new Exception("指定sql语句的参数类型必须为string");
                    }
                    gen.Emit(OpCodes.Ldarg, paramter.Position + 1);
                }
                gen.Emit(OpCodes.Stloc, sqlText_bulider);
                MapRouteAndProvider(configuration, route_name, function_type, m, m_route_type, gen, sqlConnection_bulider, dbNetProvider, paramterBulider);
                SetCache(m.ReturnType, function_type.Name, m.Name, sqlText_bulider, gen, user_cache, result_builder, cacheKey_bulider,
                    cacheItem_bulider, cacheProvider, paramterBulider, hasCacheBulider, end_label);
                string commandType = string.Empty;
                if (m_fun != null)
                {
                    commandType = m_fun.CommandType;
                }
                //建立Command
                gen.Emit(OpCodes.Ldloc, sqlText_bulider);
                gen.Emit(OpCodes.Ldloc, sqlConnection_bulider);
                gen.Emit(OpCodes.Ldloc, paramterBulider);
                gen.Emit(OpCodes.Ldstr, commandType);
                if (m_fun == null || string.IsNullOrEmpty(m_fun.IsolationLevel))
                {
                    gen.Emit(OpCodes.Ldsfld, MethodHelper.none_level);
                }
                else
                {
                    gen.Emit(OpCodes.Ldstr, m_fun.IsolationLevel);
                    gen.Emit(OpCodes.Newobj, MethodHelper.level_bulid);
                }
                gen.Emit(OpCodes.Newobj, MethodHelper.command_bulid_Method);
                gen.Emit(OpCodes.Stloc, commandBulider);
                //处理NetScope
                if (scopeParemterInfo != null)
                {
                    gen.Emit(OpCodes.Ldarg, scopeParemterInfo.Position + 1);
                    if (scopeParemterInfo.ParameterType.IsByRef)
                    {
                        gen.Emit(OpCodes.Ldind_Ref);
                    }
                    gen.Emit(OpCodes.Stloc, netScope_bulider);
                }
                //执行命令
                LocalBuilder dbNetResultBulider = gen.DeclareLocal(typeof(DbNetResult));
                gen.Emit(OpCodes.Ldloc, dbNetProvider);
                gen.Emit(OpCodes.Ldloc, commandBulider);
                gen.Emit(OpCodes.Ldloc, netScope_bulider);
                gen.Emit(OpCodes.Ldc_I4, (int)executeType);
                gen.Emit(OpCodes.Call, MethodHelper.db_exec_Method);
                gen.Emit(OpCodes.Stloc, dbNetResultBulider);
                gen.Emit(OpCodes.Ldloc, dbNetResultBulider);
                Type rType = m.ReturnType;
                if (rType.IsValueType || rType == typeof(string)
                    || (rType.IsArray && rType.GetElementType() != null && rType.GetElementType().IsValueType))
                {
                    gen.Emit(OpCodes.Call, MethodHelper.get_result_method.MakeGenericMethod(rType));
                }
                else
                {
                    gen.Emit(OpCodes.Call, MethodHelper.get_result_method.MakeGenericMethod(typeof(DataSet)));
                    if (rType != typeof(DataSet) &&
                        rType != typeof(DataTable))
                    {
                        //需要做List转换的类型
                        if (rType.GetInterface(typeof(System.Collections.IList).FullName) != null)
                        {
                            gen.Emit(OpCodes.Call, MethodHelper.to_list_Method.MakeGenericMethod(rType.GetGenericArguments()));
                        }
                        else if (rType.IsArray)
                        {
                            gen.Emit(OpCodes.Call, MethodHelper.to_array_Method.MakeGenericMethod(rType.GetElementType()));
                        }
                        else if (rType.IsClass)
                        {
                            gen.Emit(OpCodes.Call, MethodHelper.to_object_Method.MakeGenericMethod(rType));
                        }
                    }
                }
                gen.Emit(OpCodes.Stloc, result_builder);

                //若存在scope输出参数则赋值
                if (scopeParemterInfo != null &&
                    (scopeParemterInfo.IsOut ||
                    scopeParemterInfo.ParameterType.IsByRef))
                {
                    gen.Emit(OpCodes.Ldarg, scopeParemterInfo.Position + 1);
                    gen.Emit(OpCodes.Ldloc, netScope_bulider);
                    gen.Emit(OpCodes.Stind_Ref);
                }
                //缓存添加
                if (user_cache)
                {
                    int dtime = 0;
                    int ntime = 0;
                    if (m_fun != null)
                    {
                        dtime = m_fun.DuringTime;
                        ntime = m_fun.CacheTime;
                    }
                    gen.Emit(OpCodes.Ldloc, commandBulider);
                    gen.Emit(OpCodes.Ldloc, result_builder);
                    gen.Emit(OpCodes.Newobj, typeof(SQLCacheItem).GetConstructor(Type.EmptyTypes));
                    gen.Emit(OpCodes.Stloc, cacheItem_bulider);
                    gen.Emit(OpCodes.Ldloc, cacheItem_bulider);
                    gen.Emit(OpCodes.Call, MethodHelper.cache_item_bulid.MakeGenericMethod(new Type[] { m.ReturnType }));
                    gen.Emit(OpCodes.Ldloc, cacheProvider);
                    gen.Emit(OpCodes.Ldloc, cacheKey_bulider);
                    gen.Emit(OpCodes.Ldloc, cacheItem_bulider);
                    gen.Emit(OpCodes.Ldc_I4, ntime);
                    gen.Emit(OpCodes.Ldc_I4, dtime);
                    gen.Emit(OpCodes.Call, MethodHelper.cache_addCacheMethod);
                }
                gen.BeginCatchBlock(typeof(Exception));
                //catch处理
                gen.Emit(OpCodes.Stloc, exceptionBulider);
                gen.Emit(OpCodes.Ldloc, exceptionBulider);
                gen.Emit(OpCodes.Call, MethodHelper.exceptionMethod);
                gen.EndExceptionBlock();

                gen.MarkLabel(end_label);

                //若有out参数返回值在此处理
                foreach (var item in outputParamters)
                {
                    string name = item.Item1;
                    int index = item.Item2;
                    MethodInfo pro_set = item.Item3;
                    Type tType = item.Item4;
                    if (pro_set==null)
                    {
                        gen.Emit(OpCodes.Ldarg, index);
                        gen.Emit(OpCodes.Ldloc, paramterBulider);
                        gen.Emit(OpCodes.Ldstr, name);
                        gen.Emit(OpCodes.Call, MethodHelper.getValueMethod.MakeGenericMethod(tType));
                        SetRef(gen, tType);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldarg, index);
                        gen.Emit(OpCodes.Ldloc, paramterBulider);
                        gen.Emit(OpCodes.Ldstr, name);
                        gen.Emit(OpCodes.Call, MethodHelper.getValueMethod.MakeGenericMethod(tType));
                        gen.Emit(OpCodes.Call, pro_set);
                    }
                }
                
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
        /// 缓存执行设置
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="user_cache"></param>
        /// <param name="result_builder"></param>
        /// <param name="cacheKey_bulider"></param>
        /// <param name="cacheItem_bulider"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="paramterBulider"></param>
        /// <param name="hasCacheBulider"></param>
        /// <param name="end_label"></param>
        private static void SetCache(Type returnType,string functionName,string methodName,LocalBuilder sqlTextBulider,ILGenerator gen, bool user_cache, LocalBuilder result_builder,
            LocalBuilder cacheKey_bulider, LocalBuilder cacheItem_bulider, LocalBuilder cacheProvider, LocalBuilder paramterBulider, LocalBuilder hasCacheBulider, Label end_label)
        {
            Label cacheLabel = gen.DefineLabel();
            if (user_cache)
            {
                #region 调用创建缓存键方法创建缓存键
                gen.Emit(OpCodes.Ldloc, cacheProvider);
                gen.Emit(OpCodes.Ldstr, functionName);
                gen.Emit(OpCodes.Ldstr, methodName);
                gen.Emit(OpCodes.Ldloc, sqlTextBulider);
                gen.Emit(OpCodes.Ldloc, paramterBulider);
                gen.Emit(OpCodes.Call, MethodHelper.cache_getKeyMethod);
                gen.Emit(OpCodes.Stloc, cacheKey_bulider);
                #endregion

                #region 调用方法获取缓存，若获取到缓存则不执行数据库命令
                gen.Emit(OpCodes.Ldloc, cacheProvider);
                gen.Emit(OpCodes.Ldloc, cacheKey_bulider);
                gen.Emit(OpCodes.Ldloca, hasCacheBulider);
                gen.Emit(OpCodes.Call, MethodHelper.cache_getCacheMethod);
                gen.Emit(OpCodes.Stloc, cacheItem_bulider);
                gen.Emit(OpCodes.Ldloc, hasCacheBulider);
                gen.Emit(OpCodes.Brfalse, cacheLabel);
                #endregion

                //根据缓存取值
                Label cacheItemNullLabel = gen.DefineLabel();
                //判断返回的SqlCacheItem是否为空，若为空无需取值，直接返回
                gen.Emit(OpCodes.Ldloc, cacheItem_bulider);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brtrue, cacheItemNullLabel);
                //取得SqlItem内的参数以及结果
                gen.Emit(OpCodes.Ldloc, cacheItem_bulider);
                gen.Emit(OpCodes.Call, MethodHelper.cacheItem_getParamters);
                gen.Emit(OpCodes.Stloc, paramterBulider);
                gen.Emit(OpCodes.Ldloc, cacheItem_bulider);
                gen.Emit(OpCodes.Call, MethodHelper.cacheItem_getResult.MakeGenericMethod(returnType));
                gen.Emit(OpCodes.Stloc, result_builder);
                gen.MarkLabel(cacheItemNullLabel);
                gen.Emit(OpCodes.Br, end_label);
                gen.MarkLabel(cacheLabel);
            }
        }

        /// <summary>
        /// 创建路由获取数据库连接以及数据库提供程序
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="route_name"></param>
        /// <param name="function_type"></param>
        /// <param name="m"></param>
        /// <param name="m_route_type"></param>
        /// <param name="gen"></param>
        /// <param name="sqlConnection_bulider"></param>
        /// <param name="dbNetProvider"></param>
        /// <param name="paramterBulider"></param>
        private static void MapRouteAndProvider(DbNetConfiguration configuration, string route_name, Type function_type, MethodInfo m, Type m_route_type, ILGenerator gen, LocalBuilder sqlConnection_bulider, LocalBuilder dbNetProvider, LocalBuilder paramterBulider)
        {
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
            if (configuration.DbProvider != null)
            {
                //执行路由结束，判断返回的数据库提供程序是否为null，若为null则根据默认设置创建数据库提供程序
                Label dbProviderNullLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, dbNetProvider);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brfalse, dbProviderNullLabel);
                //创建默认的提供程序
                gen.Emit(OpCodes.Newobj, configuration.DbProvider.GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, dbNetProvider);
                gen.MarkLabel(dbProviderNullLabel);
            }
            //若不存在默认设置则抛出异常
            Label dbProviderNullLabel_two = gen.DefineLabel();
            gen.Emit(OpCodes.Ldloc, dbNetProvider);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, dbProviderNullLabel_two);
            gen.ThrowException(typeof(DbProviderNotFoundException));
            gen.MarkLabel(dbProviderNullLabel_two);
        }

        /// <summary>
        /// 获取参数封装入集合
        /// </summary>
        /// <param name="paramterList"></param>
        /// <param name="gen"></param>
        /// <param name="paramterListBulider"></param>
        private static void GetParamters(List<Tuple<string, int, MethodInfo,Type>> outputParamters,
            ParameterInfo[] paramterList, ILGenerator gen,LocalBuilder paramterListBulider,bool use_cache,string sqlTextKey,out ParameterInfo scope_parameterInfo)
        {
            scope_parameterInfo = null;
            bool haschache_attr = paramterList.Any(x => x.GetCustomAttribute<DbCacheKeyAttribute>() != null);
            foreach (var paramter in paramterList)
            {
                if (!string.IsNullOrEmpty(sqlTextKey) && paramter.Name == sqlTextKey)
                {
                    //跳过指定sql语句的参数
                    continue;
                }
                Type pType = paramter.ParameterType;
                Type eType = pType.GetElementType();//若为ref或out参数则eType为其原来类型
                if (pType.GetInterface(SCOPE_ITEM) != null||
                    (eType!=null&&eType.GetInterface(SCOPE_ITEM) !=null))
                {
                    //IDbNetScope类型为事务处理变量，不能添加到参数中
                    scope_parameterInfo = paramter;
                    continue;
                }
                LocalBuilder pTypeBulider = null;
                DbNetParamterDirection dir = DbNetParamterDirection.Input;
                CacheKeyType cacheKeyType = CacheKeyType.None;
                if (use_cache)
                {
                    cacheKeyType = CacheKeyType.Bind;
                    if (haschache_attr)
                    {
                        cacheKeyType = CacheKeyType.None;
                    }
                    var cache_attr = paramter.GetCustomAttribute<DbCacheKeyAttribute>();
                    if (cache_attr != null)
                    {
                        cacheKeyType = CacheKeyType.Bind;
                    }
                }
                if (!paramter.IsOut &&
                    !pType.IsByRef)
                {
                    pTypeBulider = gen.DeclareLocal(pType);
                    gen.Emit(OpCodes.Ldarg, paramter.Position + 1);
                }
                else
                {
                    dir = DbNetParamterDirection.InputAndOutPut;
                    pTypeBulider = gen.DeclareLocal(eType);
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
                if (paramter.IsOut || pType.IsByRef)
                {
                    //输出参数获取下阶段备用
                    outputParamters.Add(new Tuple<string, int, MethodInfo, Type>(paramter.Name, paramter.Position + 1, null, eType));
                }
                if (pTypeBulider.LocalType == typeof(string) ||
                    pTypeBulider.LocalType.IsValueType ||
                    pTypeBulider.LocalType.IsArray)
                {
                    //添加结构体或者string到集合
                    gen.Emit(OpCodes.Ldloc, paramterListBulider);
                    gen.Emit(OpCodes.Ldstr, paramter.Name);
                    gen.Emit(OpCodes.Ldloc, pTypeBulider);
                    gen.Emit(OpCodes.Ldc_I4, (int)dir);
                    gen.Emit(OpCodes.Ldc_I4, paramter.Position);
                    gen.Emit(OpCodes.Ldc_I4, (int)SourceType.FromArg);
                    gen.Emit(OpCodes.Ldc_I4, (int)cacheKeyType);
                    gen.Emit(OpCodes.Call, MethodHelper.paramterMethod.MakeGenericMethod(pTypeBulider.LocalType));
                }
                else
                {
                    //如果参数是类则需进一步处理
                    var pinfo = pTypeBulider.LocalType.GetProperties();
                    var cache_attr_c = paramter.GetCustomAttribute<DbCacheKeyAttribute>();
                    foreach (var p in pinfo)
                    {
                        DbParamterAttribute attr_p = p.GetCustomAttribute<DbParamterAttribute>(true);
                        bool except = false;
                        string name = string.Empty;
                        DbNetParamterDirection pdir = DbNetParamterDirection.Input;
                        CacheKeyType p_cacheKeyType = CacheKeyType.None;
                        if (use_cache)
                        {
                            p_cacheKeyType = CacheKeyType.Bind;
                            if (haschache_attr)
                            {
                                p_cacheKeyType = CacheKeyType.None;
                            }
                            if (cache_attr_c != null)
                            {
                                p_cacheKeyType = CacheKeyType.Bind;
                            }
                            if (attr_p != null&&attr_p.CacheKey!=CacheKeyType.Default)
                            {
                                p_cacheKeyType = attr_p.CacheKey;
                            }
                        }
                        if (paramter.IsOut)
                        {
                            //输出参数获取下阶段备用
                            outputParamters.Add(new Tuple<string, int, MethodInfo,Type>(paramter.Name, paramter.Position + 1,p.GetSetMethod(),p.PropertyType));
                            pdir = DbNetParamterDirection.Output;
                        }
                        else if (pType.IsByRef)
                        {
                            //输出参数获取下阶段备用
                            outputParamters.Add(new Tuple<string, int, MethodInfo, Type>(paramter.Name, paramter.Position + 1, p.GetSetMethod(), p.PropertyType));
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
                            gen.Emit(OpCodes.Ldc_I4, (int)p_cacheKeyType);
                            gen.Emit(OpCodes.Call, MethodHelper.paramterMethod.MakeGenericMethod(p.PropertyType));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取需要执行的命令类型
        /// </summary>
        /// <param name="m"></param>
        /// <param name="executeType"></param>
        private void GetExecuteType(MethodInfo m, ref ExecuteType executeType)
        {
            var rType = m.ReturnType;
            if (executeType == ExecuteType.Default)
            {
                //执行命令类型的默认设置
                if (rType != null)
                {
                    if (rType == typeof(int))
                    {
                        executeType = ExecuteType.ExecuteNoQuery;
                    }
                    else if (rType.IsValueType || rType == typeof(string)
                        ||(rType.IsArray&& rType.GetElementType()!=null&&rType.GetElementType().IsValueType))
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
