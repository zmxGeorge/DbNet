using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 参数方向
    /// </summary>
    public enum DbNetParamterDirection
    {
        /// <summary>
        /// 输入参数
        /// </summary>
        Input=0,
        /// <summary>
        /// 输出参数
        /// </summary>
        Output=1,
        /// <summary>
        /// 即是输出也是输出参数
        /// </summary>
        InputAndOutPut=2
    }
}
