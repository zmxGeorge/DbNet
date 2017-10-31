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
        private readonly DbNetCommand _dbNetCommand;

        private readonly object _result;

        public SQLCacheItem(DbNetCommand command, object result)
        {
            _dbNetCommand = command;
            _result = result;
        }

        /// <summary>
        /// 获取参数缓存的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramterKey"></param>
        /// <returns></returns>
        public T GetParamterValue<T>(string paramterKey)
        {
            if (_dbNetCommand == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(_dbNetCommand.Paramters.Get(paramterKey), typeof(T));
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
