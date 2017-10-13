using System;
using System.Collections.Generic;
using System.Text;

namespace DbNet
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DbRouteKeyAttribute:Attribute
    {
        /// <summary>
        /// 获取具有指向性索引值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object GetValue<T>(T value)
        {
            return value;
        }
    }
}
