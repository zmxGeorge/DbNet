﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DbNet
{
    public class SqlServerDbProvider : IDbNetProvider
    {
        public SqlServerDbProvider()
        {
        }

        public DbNetResult ExecuteCommand(DbNetCommand command,ref IDbNetScope scope, ExecuteType executetype)
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
