using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbNet;

namespace Test
{
    public class User
    {
        [DbParamter(Name ="uid")]
        public int Id { get; set; }

        [DbParamter(Name ="username")]
        public string Name { get; set; }

        [DbParamter(Name ="password")]
        public string PassWord { get; set; }
    }

    public interface IUser:IDbFunction
    {
        /// <summary>
        /// 该接口例子是
        /// 使用事务级别为ReadCommitted，
        /// 有缓存设置，且缓存未被访问300秒时缓存过期
        /// DbCacheKey为说明哪个参数时缓存的关键参数
        /// name参数为无用参数，用以说明DbCacheKey的作用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="netScope"></param>
        /// <returns></returns>
        [DbFunction(SqlText ="select top 10 * from _user where uid=@id",
            ExecuteType =ExecuteType.ExecuteDateTable,CommandType ="SqlText",UserCache =true,DuringTime =300,
            UseTransaction =true,IsolationLevel = "ReadCommitted")]
        User GetUserById([DbCacheKey]int id,string name,out SqlServerDbNetScope netScope);
    }
}
