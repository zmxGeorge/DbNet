using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    /// <summary>
    /// 缓存关键字特性
    /// 注：当该特性存在与参数中，若需要缓存，则默认其他都不是缓存关键字
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DbCacheKeyAttribute:Attribute
    {
    }
}
