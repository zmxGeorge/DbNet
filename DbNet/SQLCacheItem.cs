using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{
    public interface ISQLCacheItem
    {
        DbNetParamterCollection GetParamters();

        T GetResult<T>();
    }

    /// <summary>
    /// 缓存项目
    /// </summary>
    public class SQLCacheItem<T>:ISQLCacheItem
    {
        private readonly DbNetCommand _dbNetCommand;

        private readonly object _result;

        public SQLCacheItem(DbNetCommand command, T result)
        {
            _dbNetCommand = command;
            _result = result;
        }


        /// <summary>
        /// 获取缓存的参数
        /// </summary>
        /// <returns></returns>
        public DbNetParamterCollection GetParamters()
        {
            return _dbNetCommand.Paramters;
        }


        public TResult GetResult<TResult>()
        {
            return (TResult)_result;
        }
    }
}
