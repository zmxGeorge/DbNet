using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbNet;

namespace Test
{
    public class M
    {
        public M()
        {
            A = "5";
            B = DateTime.Now;
            C = 6;
        }

        public string A { get; set; }

        public DateTime B { get; set; }

        public int C { get; set; }
    }

    public interface IUser:IDbFunction
    {
        [DbFunction(SqlText ="select * from user username=@name password=@password",
            ExecuteType =ExecuteType.ExecuteObject,CommandType ="SqlText",UserCache =true)]
        string LoginUser(string name,string password,[DbCacheKey]int id,DateTime dateTime,byte[] data,Guid guid,ref M m);
    }
}
