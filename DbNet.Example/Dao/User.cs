using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbNet;

namespace DbNet.Example.Dao
{
    
    /// <summary>
    /// 与User表对应的模型
    /// 注意，如果返回值时实体集或单个实体，
    /// 属性需要和结果集字段一一对应，默认依据属性名称和字段名称对应
    /// 如属性名称和字段名称不一致，则可以通过DbParamter去绑定对应关系
    /// </summary>
    public class User
    {
        [DbParamter(Name ="Id")]
        public int UserId { get; set; }

        
        public string Name { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }
    }
}
