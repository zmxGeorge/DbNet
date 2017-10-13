using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbNet
{
    public class SqlServerDbNetScope : DbNetScope
    {
        private string _connection_string = null;

        private DbNetIsolationLevel _isolationLevel = null;

        public SqlConnection Connection { get; set; }

        public SqlTransaction Transaction { get; set; }


        public SqlServerDbNetScope(string connectionString,DbNetIsolationLevel isolationLevel)
        {
            _connection_string = connectionString;
            _isolationLevel = isolationLevel;
        }

        public override void Close()
        {
            if (Transaction == null)
            {
                Dispose();
            }
        }

        public override void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
            }
        }

        public override void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }
            if (Connection != null)
            {
                Connection.Dispose();
            }
        }

        /// <summary>
        /// 打开连接并开启事务
        /// 注意，若外部传入的事务级别与当前事务级别不同
        /// 则事务会被提交并创建新事务
        /// </summary>
        public override void Open()
        {
            if (Connection == null)
            {
                Connection = new SqlConnection(_connection_string);
            }
            if (Connection.State == ConnectionState.Broken ||
                Connection.State==ConnectionState.Closed)
            {
                Connection.Open();
            }
            IsolationLevel level = IsolationLevel.Unspecified;
            if (_isolationLevel != null&&
                _isolationLevel!=DbNetIsolationLevel.None)
            {
                switch (_isolationLevel.Level)
                {
                    case "Unspecified":
                        level=IsolationLevel.Unspecified;
                        break;
                    case "Chaos":
                        level = IsolationLevel.Chaos;
                        break;
                    case "ReadUncommitted":
                        level = IsolationLevel.ReadUncommitted;
                        break;
                    case "ReadCommitted":
                        level = IsolationLevel.ReadCommitted;
                        break;
                    case "RepeatableRead":
                        level = IsolationLevel.RepeatableRead;
                        break;
                    case "Serializable":
                        level = IsolationLevel.Serializable;
                        break;
                    case "Snapshot":
                        level = IsolationLevel.Snapshot;
                        break;
                    default:
                        level = IsolationLevel.Unspecified;
                        break;
                }
            }
            if (_isolationLevel != DbNetIsolationLevel.None)
            {
                if (Transaction != null && Transaction.IsolationLevel != level)
                {
                    Transaction.Commit();
                    Transaction.Dispose();
                    Transaction = null;
                }
                if (Transaction == null)
                {
                    Transaction = Connection.BeginTransaction(level);
                }
            }
        }

        public override void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
            }
        }
    }
}
