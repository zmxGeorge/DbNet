using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 参数集合
    /// </summary>
    public class DbNetParamterCollection:IEnumerable<DbNetParamter>
    {
        private readonly Dictionary<string, DbNetParamter> dic = new Dictionary<string, DbNetParamter>();

        public void Add<T>(string key, T value, DbNetParamterDirection direction)
        {
            dic.Add(key,new DbNetParamter { Name = key, Value = value, Direction = direction });
        }

        public DbNetParamter Get(string key)
        {
            if (!dic.ContainsKey(key))
            {
                return null;
            }
            else
            {
                return dic[key];
            }
        }

        public IEnumerator<DbNetParamter> GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }

        public DbNetParamter UpdateValue<T>(string key, T value)
        {
            if (!dic.ContainsKey(key))
            {
                var p = new DbNetParamter { Direction = DbNetParamterDirection.InputAndOutPut, Name = key, Value = value };
                dic.Add(key, p);
                return p;
            }
            else
            {
                var p = dic[key];
                p.Value = value;
                return p;
            }
        }

    }
}
