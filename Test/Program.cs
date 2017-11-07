using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbNet;

namespace Test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            SqlServerSimpleDbRouteProvider.SetDefaultConnectionString("Data Source = 123.207.183.51;Initial Catalog = yiqi_formal;User Id = dbtest;Password = q1w2e3r4;");//设置数据库连接
            DbNetConfiguration.AddDbProvider<SqlServerDbProvider>();//设置SqlServer数据库提供程序
            DbNetConfiguration.MapRoute<SqlServerSimpleDbRouteProvider>("*");//设置数据库默认路由，注:默认路由名称为*
            DbNetConfiguration.AddCacheProvider<MemoryCacheProvider>();//设置缓存提供程序
            DbNetConfiguration.AddFunctionProvider<ILDbNetFunctionProvider>();//设置接口注入提供程序
            DbNetConfiguration.RegistFunction<IUser>();//注册接口
            var user = DbNetConfiguration.GetFunction<IUser>();//获取接口实现实例
            var r=user.GetUserById(3000002,string.Empty,out SqlServerDbNetScope netScope);//带事务的调用
            netScope.Commit();//提交事务
            DbContext.SqlException += DbContext_SqlException;//接口异常事件处理，接口中任何异常都会触发该事件
        }

        private static void DbContext_SqlException(Exception e)
        {
            
        }
    }
}
