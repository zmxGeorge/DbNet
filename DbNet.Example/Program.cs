using DbNet.Example.Dao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string connect_string_format = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True";
            SqlServerSimpleDbRouteProvider.SetDefaultConnectionString(string.Format(connect_string_format,Path.GetFullPath("test.mdf")));//设置数据库连接
            DbNetConfiguration.AddDbProvider<SqlServerDbProvider>();//设置SqlServer数据库提供程序
            DbNetConfiguration.MapRoute<SqlServerSimpleDbRouteProvider>("*");//设置数据库默认路由，注:默认路由名称为*
            DbNetConfiguration.AddCacheProvider<MemoryCacheProvider>();//设置缓存提供程序
            DbNetConfiguration.AddFunctionProvider<ILDbNetFunctionProvider>();//设置接口注入提供程序
            DbNetConfiguration.RegistFunction<IUserDao>();//注册接口
            DbContext.SqlException += DbContext_SqlException;//接口异常事件处理，接口中任何异常都会触发该事件

            //获取DbNet自动实现该接口的实例
            var dao = DbNetConfiguration.GetFunction<IUserDao>();

            #region 普通查询

            //原生不带实体类转换查询
            var set = dao.GetUsers();

            //带实体类转换查询
            var list1 = dao.GetUsersToList();

            //参数化查询
            var list2 = dao.GetUsersToListByAge(18);

            //带实例参数查询
            User u3 = new User();
            u3.UserId = 2;
            var list3 = dao.GetUsersToListBySimple(u3);

            //参数字符串拼装查询
            //{@age}相当于参数的一个占位符，使用string.Format填入参数值的效果
            //该方式会存有Sql注入漏洞，谨慎使用
            var list4 = dao.GetUsersToListByAgeTxt(18);

            //输入和输出参数的支持
            int c = 0;
            var list5 = dao.GetUsersToListByAge(18,ref c);

            //可选参数查询
            int? a4 = null;
            var list6 = dao.GetUsersToListByAge(a4);

            //查询返回单个值
            var c7 = dao.GetUserCount();
            #endregion

            #region 事务使用
            SqlServerDbNetScope scope = null;
            //注意:单个事务处理用out，连续性事务处理用ref
            //ref情况下，若scope本身为null，则会自动创建，不为null则使用此实例
            //SqlServerDbNetScope内部保存的正是数据库连接实例和事务实例
            //Dispose必须在事务完成之后调用，否则不能回收数据库连接
            var list8 = dao.GetUsersToListForTran(ref scope);
            var list9 = dao.GetUsersToListForTran(ref scope);
            scope.Commit();
            scope.Dispose();
            #endregion
            //缓存使用，实际调用MemoryCacheProvider，在DbNet.MemoryCache里面
            var lis10 = dao.GetUsersToListForCache();
            Console.ReadLine();
        }

        private static void DbContext_SqlException(Exception e)
        {
            /*
             * 每个接口中实现的方法都已添加了try catch,这就不用手动添加
             * 因此所有因执行该方法引发的异常都可以通过这个事件进行处理
             */
        }
    }
}
