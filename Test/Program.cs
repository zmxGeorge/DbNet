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
            SqlServerSimpleDbRouteProvider.SetDefaultConnectionString("Data Source = 123.207.183.51;Initial Catalog = yiqi_formal;User Id = dbtest;Password = q1w2e3r4;");
            DbNetConfiguration.AddDbProvider<SqlServerDbProvider>();
            DbNetConfiguration.MapRoute<SqlServerSimpleDbRouteProvider>("*");
            DbNetConfiguration.AddCacheProvider<MemoryCacheProvider>();
            DbNetConfiguration.AddFunctionProvider<ILDbNetFunctionProvider>();
            DbNetConfiguration.RegistFunction<IUser>();
            var user = DbNetConfiguration.GetFunction<IUser>();
            M m = null;
            SqlServerDbNetScope a1 = null;
            var id = Guid.NewGuid();
            var r=user.LoginUser("name23123", "pwd",0,DateTime.Now,null,ref id,ref m,out a1);
        }


    }
}
