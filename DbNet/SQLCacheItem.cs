using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{
    /// <summary>
    /// 缓存项目
    /// </summary>
    public class SQLCacheItem
    {
        private DbNetCommand _dbNetCommand;

        private object _result;

        public SQLCacheItem()
        {
            
        }

        public void SetItem<TResult>(DbNetCommand command, TResult result)
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


        /// <summary>
        /// 获取结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetReult<T>()
        {
            return (T)_result;
        }
    }
}
