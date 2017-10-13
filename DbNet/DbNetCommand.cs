using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    public class DbNetCommand
    {
        /// <summary>
        /// 执行的数据库命令语句
        /// </summary>
        public string SqlText { get; set; }

        /// <summary>
        /// 获取参数集合
        /// </summary>
        public DbNetParamterCollection Paramters { get;set; }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public string ConnectionString { get;set; }

        /// <summary>
        /// 执行类型
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// 事务执行级别
        /// </summary>
        public DbNetIsolationLevel IsolationLevel { get;set; }

        public DbNetCommand(string _sqlText,string _connectionString, DbNetParamterCollection _paramters,string _commandType, DbNetIsolationLevel _isolationLevel)
        {
            SqlText = _sqlText;
            ConnectionString = _connectionString;
            Paramters = _paramters;
            CommandType = _commandType;
            IsolationLevel = _isolationLevel;
        }
    }
}
