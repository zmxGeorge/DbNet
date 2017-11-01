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
    public class SqlServerSimpleDbRouteProvider : IDbNetRouteProvider
    {
        private static string _connectionString = null;

        private static readonly SqlServerDbProvider _dbProvider = new SqlServerDbProvider();

        public SqlServerSimpleDbRouteProvider()
        {
        }

        public static void SetDefaultConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string RouteDbConnection(string routeName, string moduleName, string methodName, DbNetParamterCollection paramters, ref IDbNetProvider dbProvider)
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
