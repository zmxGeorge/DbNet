using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 查询参数
    /// </summary>
    public class DbNetParamter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 参数方向
        /// </summary>
        public DbNetParamterDirection Direction { get; set; }

        /// <summary>
        /// 目标参数位置
        /// </summary>
        public int SourceIndex { get; set; }

        /// <summary>
        /// 目标参数源类型
        /// </summary>
        public SourceType SourceType { get; set; }
    }
}
