using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 简单SqlServer数据库路由提供程序
    /// 不带分布式处理
    /// </summary>
    public class SqlServerSimpleDbRouteProvider : DbNetRouteProvider
    {
        private static string _connectionString = null;

        private static readonly SqlServerDbProvider _dbProvider = new SqlServerDbProvider();

        public static void SetDefaultConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override string RouteDbConnection(string routeName, string moduleName, string methodName, Dictionary<string, object> paramters, ref DbNetProvider dbProvider)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new Exception("默认数据库连接字符串未设置");
            }
            if (dbProvider == null)
            {
                dbProvider = _dbProvider;
            }
            return _connectionString;
        }
    }
}
