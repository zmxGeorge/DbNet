using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbNet;

namespace DbNet.Example.Dao
{
    /// <summary>
    /// 注意所有需要使用DbNet框架的接口必须继承IDbFunction接口
    /// </summary>
    public interface IUserDao:IDbFunction
    {

        /*
         * 特别说明:
         * 所有接口中方法定义必须有DbFunction特性
         * 
         * 1.SqlText即是Sql语句，或者存储过程名称
         * 
         * 2.定义中方法的参数即是需要使用的参数
         * 例如参数为age,则Sql语句中@age才会有效，区分大小写
         * 支持参数中包含实体类，或获取所有属性的值
         * 若有DbParamter特性，则使用其特性名称为参数的key
         * 若没有则使用属性名称为参数的key
         * 具体参考，GetUsersToListBySimple定义
         * 属性名称UserId,参数使用用DbParamter特性上的Id
         * 
         * 3.缓存则需要初始化时注册缓存模块，特性上的设置才会有效
         * 
         * 4.返回值支持:值类型，单个实体类，List,DataSet
         * 实体类转换不支持复杂类转换,就是类内部属性不能再套一个类
         * 
         * 5.执行的时候默认,ExecuteType.Default
         * 这里有个细节，如果返回值设置为int,则默认执行ExecuteNoQuery
         * 如果就是想要返回的就是一个int值，则ExecuteType需设置为ExecuteType.ExecuteObject
         * 
         * CommandType，IsolationLevel，之所以采用string，是为了扩展性
         * CommandType和IsolationLevel肯定能支持更多情况，因此不设限
         */

        [DbFunction(SqlText ="SELECT * FROM [User]")]
        DataSet GetUsers();

        [DbFunction(SqlText = "SELECT COUNT(*) FROM [User]",ExecuteType =ExecuteType.ExecuteObject)]
        int GetUserCount();

        [DbFunction(SqlText = "SELECT * FROM [User]")]
        List<User> GetUsersToList();

        [DbFunction(SqlText = "SELECT * FROM [User]",
            UseTransaction =true,
            IsolationLevel = "ReadCommitted")]
        List<User> GetUsersToListForTran(ref SqlServerDbNetScope scope);

        [DbFunction(SqlText = "SELECT * FROM [User]",UserCache =true,DuringTime =300)]
        List<User> GetUsersToListForCache();

        [DbFunction(SqlText = "SELECT * FROM [User] WHERE Age=@age")]
        List<User> GetUsersToListByAge(int age);

        [DbFunction(SqlText = "SELECT * FROM [User] WHERE Id=@Id")]
        List<User> GetUsersToListBySimple(User user);

        [DbFunction(SqlText = "SELECT * FROM [User] WHERE Age={@age}")]
        List<User> GetUsersToListByAgeTxt(int age);
        /// <summary>
        /// 注意可选参数?和参数名称必须连着，中间不能有空格,
        /// 例如：? age不会被识别，会保存,且必须用括号包裹
        /// 若不使用该条件age为NULL即可
        /// 类参数也支持该选项
        /// </summary>
        /// <param name="age"></param>
        /// <returns></returns>
        [DbFunction(SqlText = "SELECT * FROM [User] WHERE (Age = ?age)")]
        List<User> GetUsersToListByAge(int? age);

        [DbFunction(SqlText = "SELECT * FROM [User] WHERE Age=@age;SET @count=(SELECT COUNT(*) FROM [User])")]
        List<User> GetUsersToListByAge(int age,ref int count);

        [DbFunction(SqlText ="_queryUserCount",CommandType= "StoredProcedure",ExecuteType =ExecuteType.ExecuteObject)]
        int Do_queryUserCount();
    }
}
