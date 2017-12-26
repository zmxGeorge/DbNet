using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    public interface IDbNetSqlParser
    {
        /// <summary>
        /// 表示该sql语句是否需要被分析
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool CanParser(string sql);

        /// <summary>
        /// 通过自定义语法将sql进行转换
        /// 默认转换规则,如下:
        /// [#参数]表示占位符,该参数值将会填充sql语句
        /// {:temp,s,[$|^|*]} 表示将填充参数名称到sql语句,
        /// :temp是填充模板,不填默认为所有参数名称,
        /// temp 中可以填 key.* key表示某个参数，通常情况下这在参数key为某个实体类中用上
        /// 若为key表示将填充该参数的类型名称,若为key.*表示将填入该参数下所有属性名称
        /// s表示分隔符号,默认为逗号,[$|^|*]表示 若填$或不填表示分隔符去尾巴,若填^表示分隔符去头,若填*表示不进行去头或去尾的操作
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        string Parse(string sql);
    }
}
