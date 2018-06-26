using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace DbNet
{
    public class SQLiteDbNetScope : IDbNetScope
    {
        private string _connection_string = null;

        private DbNetIsolationLevel _isolationLevel = null;

        private bool is_commit = false;

        public SQLiteConnection Connection { get; set; }

        public SQLiteTransaction Transaction { get; set; }


        public SQLiteDbNetScope(string connectionString,DbNetIsolationLevel isolationLevel)
        {
            _connection_string = connectionString;
            _isolationLevel = isolationLevel;
        }

        public void Close()
        {
            if (Transaction == null)
            {
                Dispose();
            }
        }

        public void Commit()
        {
            if (Transaction != null)
            {
                is_commit = true;
                Transaction.Commit();
            }
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                if (!is_commit)
                {
                    //若事务未提交，则会自动提交事务
                    try
                    {
                        Transaction.Commit();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
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
        public void Open()
        {
            if (Connection == null)
            {
                Connection = new SQLiteConnection(_connection_string);
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

        public void Rollback()
        {
            if (Transaction != null)
            {
                is_commit = true;
                Transaction.Rollback();
            }
        }
    }
}
