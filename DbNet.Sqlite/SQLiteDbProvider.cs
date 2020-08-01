using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Data.SQLite;

namespace DbNet
{
    public class SQLiteDbProvider : IDbNetProvider
    {
        private const string PARAMTERFORAMT = "@{0}";

        private static readonly Regex PARAMTER_REPLACE = new Regex(@"[\?](?<pName>[\w]+)",RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

        private static readonly Regex FORMAT_PARAMTER_REPLACE = new Regex(@"[\{][\@](?<pName>[\w]+)[\}]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private const string FORMAT_CODE = "{{@{0}}}";

        private const string P_NAME = "pName";

        public SQLiteDbProvider()
        {
        }

        public DbNetResult ExecuteCommand(DbNetCommand command,ref IDbNetScope scope, ExecuteType executetype)
        {
            StringBuilder sqlBulider =new StringBuilder(PARAMTER_REPLACE.Replace(command.SqlText, "@${pName} OR @${pName} IS NULL"));
            string sql = sqlBulider.ToString();
            foreach (Match m in FORMAT_PARAMTER_REPLACE.Matches(sql))
            {
                if (m.Success)
                {
                    var name = m.Groups[P_NAME].Value;
                    var paramter = command.Paramters.Get(name);
                    if (paramter == null)
                    {
                        //如果本身不存在参数集合里面则跳过
                        continue;
                    }
                    string res = val == null ? string.Empty : val.ToString();
                    var code = string.Format(FORMAT_CODE, name);
                    sqlBulider = sqlBulider.Replace(code, res);
                }
            }
            sql = sqlBulider.ToString();
            command.SqlText = sql;
            //封装数据库执行
            object result = null;
            scope = GetScope(scope,command);
            scope.Open();
            var s = scope as SQLiteDbNetScope;
            SQLiteCommand com = new SQLiteCommand(command.SqlText, s.Connection);
            if (s.Transaction != null)
            {
                com.Transaction = s.Transaction;
            }
            com.CommandTimeout = 30;
            switch (command.CommandType)
            {
                default:
                case "Text":
                    com.CommandType = CommandType.Text;
                    break;
                case "StoredProcedure":
                    com.CommandType = CommandType.StoredProcedure;
                    break;
                case "TableDirect":
                    com.CommandType = CommandType.TableDirect;
                    break;
            }
            foreach (var p in command.Paramters)
            {
                var sql_p = new SqlParameter(string.Format(PARAMTERFORAMT, p.Name), p.Value);
                if (p.Value == null)
                {
                    sql_p.Value = DBNull.Value;
                }
                switch (p.Direction)
                {
                    case DbNetParamterDirection.Input:
                        sql_p.Direction = ParameterDirection.Input;
                        break;
                    case DbNetParamterDirection.InputAndOutPut:
                        sql_p.Direction = ParameterDirection.InputOutput;
                        break;
                    case DbNetParamterDirection.Output:
                        sql_p.Direction = ParameterDirection.Output;
                        break;
                }
                com.Parameters.Add(sql_p);
            }
            switch (executetype)
            {
                case ExecuteType.ExecuteDateTable:
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(com))
                    {
                        DataSet set = new DataSet();
                        adapter.Fill(set);
                        result = set;
                    }
                    break;
                case ExecuteType.ExecuteNoQuery:
                    result = com.ExecuteNonQuery();
                    break;
                case ExecuteType.ExecuteObject:
                    result = com.ExecuteScalar();
                    break;
            }
            scope.Close();
            foreach (var p in command.Paramters)
            {
                string pName = string.Format(PARAMTERFORAMT, p.Name);
                if (com.Parameters[pName] != null)
                {
                    object pv = com.Parameters[pName].Value;
                    if (pv == DBNull.Value)
                    {
                        p.Value = null;
                    }
                    else
                    {
                        p.Value = com.Parameters[pName].Value;
                    }
                }
            }
            return new DbNetResult(result);
        }

        private IDbNetScope GetScope(IDbNetScope scope,DbNetCommand command)
        {
            if (scope != null&&scope is SQLiteDbNetScope)
            {
                return scope;
            }
            else
            {
                return new SQLiteDbNetScope(command.ConnectionString,command.IsolationLevel);
            }
        }
    }
}
