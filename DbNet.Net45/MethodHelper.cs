using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;

namespace DbNet.Net45
{
    public static class MethodHelper
    {
        public static T ParseResult<T>(object obj) where T : new()
        {
            return default(T);
        }
    }
}
