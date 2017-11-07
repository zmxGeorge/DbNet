using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    public class DbNetResult
    {
        private readonly object _result;

        public DbNetResult(object result)
        {
            _result = result;
        }

        public T Get<T>()
        {
            if (_result == null)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(_result, typeof(T));
            }
        }
    }
}
