using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace DbNet
{
    /// <summary>
    /// 参数配置项
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbParamterAttribute:Attribute
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数方向
        /// </summary>
        public DbNetParamterDirection ParameterDirection { get; set; }

        /// <summary>
        /// 是否排除改映射项
        /// </summary>
        public bool Except { get; set; }
    }
}
