using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 路由特性值
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public class DbRouteAttribute:Attribute
    {
        /// <summary>
        /// 指定路由名称
        /// </summary>
        public string Name { get; set; }

        public DbRouteAttribute(string name)
        {
            Name = name;
        }
    }
}
