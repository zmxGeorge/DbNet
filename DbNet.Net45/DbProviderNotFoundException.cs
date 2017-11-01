using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 数据库提供程序未找到引发的异常
    /// </summary>
    public class DbProviderNotFoundException:Exception
    {
        public DbProviderNotFoundException() : base("未设置数据库提供程序")
        {
        }
    }
}
