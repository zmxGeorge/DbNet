using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    public class SqlServerDbProvider : IDbNetProvider
    {
        private const string PARAMTERFORAMT = "@{0}";

        public SqlServerDbProvider()
        {
        }

        public DbNetResult ExecuteCommand(DbNetCommand command, IDbNetScope scope, ExecuteType executetype)
        {
            //封装数据库执行
            object result = null;
            scope = GetScope(scope,command);
            scope.Open();
            var s = scope as SqlServerDbNetScope;
            SqlCommand com = new SqlCommand(command.SqlText, s.Connection);
            if (s.Transaction != null)
            {
                com.Transaction = s.Transaction;
            }
            com.CommandTimeout = 30;
            foreach (var p in command.Paramters)
            {
                var sql_p = new SqlParameter(string.Format(PARAMTERFORAMT, p.Name), p.Value);
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter(com))
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
            return new DbNetResult(result);
        }

        private IDbNetScope GetScope(IDbNetScope scope,DbNetCommand command)
        {
            if (scope != null&&scope is SqlServerDbNetScope)
            {
                return scope;
            }
            else
            {
                return new SqlServerDbNetScope(command.ConnectionString,command.IsolationLevel);
            }
        }
    }
}
