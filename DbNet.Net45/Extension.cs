using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DbNet
{
    delegate List<T> ToTableList<T>(DataTable table, string suffix) where T : class, new();
    public static class Extension
    {
        private static readonly ConcurrentDictionary<Type, Delegate> cache_del = new ConcurrentDictionary<Type, Delegate>();

        public static readonly PropertyInfo RowsInfo = typeof(DataTable).GetProperty("Rows", typeof(DataRowCollection), Type.EmptyTypes);

        public static readonly PropertyInfo ColumnsInfo = typeof(DataTable).GetProperty("Columns", typeof(DataColumnCollection), Type.EmptyTypes);

        public static readonly MethodInfo StringConcat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });

        public static readonly PropertyInfo CountInfo = typeof(DataRowCollection).GetProperty("Count", typeof(int), Type.EmptyTypes);

        public static readonly MethodInfo m_get_row_Value = typeof(Extension).GetMethod("GetRowValue", BindingFlags.NonPublic | BindingFlags.Static);

        public static readonly MethodInfo m_get_row_define = typeof(Extension).GetMethod("GetRow", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// 将DataTable数据转换为List数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable table, string suffix = "") where T : class, new()
        {
            if (table == null)
            {
                return new List<T>();
            }
            Type tType = typeof(T);
            Delegate del = null;
            if (!cache_del.TryGetValue(tType, out del))
            {
                PropertyInfo[] pinfos = tType.GetProperties();
                Dictionary<string, PropertyInfo> dic_p = new Dictionary<string, PropertyInfo>();
                foreach (var p in pinfos)
                {
                    DbParamterAttribute paramter_att = p.GetCustomAttribute<DbParamterAttribute>();
                    string keyName = p.Name;
                    if (!string.IsNullOrEmpty(paramter_att.Name))
                    {
                        keyName = paramter_att.Name;
                    }
                    if (dic_p.ContainsKey(keyName))
                    {
                        throw new Exception(string.Format("列名映射关系重复:{0}", keyName));
                    }
                    else
                    {
                        dic_p.Add(keyName, p);
                    }
                }
                MethodInfo addMethod = typeof(List<T>).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                DynamicMethod method = new DynamicMethod(tType.FullName, typeof(List<T>), new Type[] { typeof(DataTable), typeof(string) }, true);
                var gen = method.GetILGenerator();
                LocalBuilder countBulider = gen.DeclareLocal(typeof(int));
                LocalBuilder tableCountBulider = gen.DeclareLocal(typeof(int));
                LocalBuilder rowBulider = gen.DeclareLocal(typeof(DataRow));
                LocalBuilder resultBulider = gen.DeclareLocal(typeof(List<T>));
                LocalBuilder newObjBulider = gen.DeclareLocal(tType);
                LocalBuilder columnBulider = gen.DeclareLocal(typeof(DataColumnCollection));
                gen.Emit(OpCodes.Newobj, typeof(List<T>).GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, resultBulider.LocalIndex);//new List
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, ColumnsInfo.GetGetMethod());
                gen.Emit(OpCodes.Stloc, columnBulider.LocalIndex);
                Dictionary<string, LocalBuilder> columnNameDic = new Dictionary<string, LocalBuilder>();
                foreach (string k in dic_p.Keys)
                {
                    if (!columnNameDic.ContainsKey(k))
                    {
                        LocalBuilder rBulider = gen.DeclareLocal(typeof(string));
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Ldstr, k);
                        gen.Emit(OpCodes.Call, StringConcat);
                        gen.Emit(OpCodes.Stloc, rBulider.LocalIndex);
                        columnNameDic.Add(k, rBulider);
                    }
                }
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, countBulider.LocalIndex);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, RowsInfo.GetGetMethod());
                gen.Emit(OpCodes.Call, CountInfo.GetGetMethod());
                gen.Emit(OpCodes.Stloc, tableCountBulider.LocalIndex);
                Label eachLabel = gen.DefineLabel();
                Label emptyLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, tableCountBulider.LocalIndex);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ble, emptyLabel);
                gen.MarkLabel(eachLabel);//循环顶部标签
                LocalBuilder entityBulider = gen.DeclareLocal(typeof(T));
                gen.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc, entityBulider.LocalIndex);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldloc, countBulider.LocalIndex);
                gen.Emit(OpCodes.Call, m_get_row_define);
                gen.Emit(OpCodes.Stloc, rowBulider.LocalIndex);
                foreach (var item in columnNameDic)
                {
                    var key = item.Key;
                    var p = dic_p[key];
                    gen.Emit(OpCodes.Ldloc, entityBulider.LocalIndex);
                    gen.Emit(OpCodes.Ldloc, rowBulider.LocalIndex);
                    gen.Emit(OpCodes.Ldloc, item.Value.LocalIndex);
                    gen.Emit(OpCodes.Call, m_get_row_Value.MakeGenericMethod(new Type[] { p.PropertyType }));
                    gen.Emit(OpCodes.Call, p.GetSetMethod());
                }
                gen.Emit(OpCodes.Ldloc, resultBulider.LocalIndex);
                gen.Emit(OpCodes.Ldloc, entityBulider.LocalIndex);
                gen.Emit(OpCodes.Call, addMethod);
                gen.Emit(OpCodes.Ldloc, countBulider.LocalIndex);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Add_Ovf);
                gen.Emit(OpCodes.Stloc, countBulider.LocalIndex);
                gen.Emit(OpCodes.Ldloc, countBulider.LocalIndex);
                gen.Emit(OpCodes.Ldloc, tableCountBulider.LocalIndex);
                //判断是否小于table.Rows.Count，是则调到循环顶部Label
                gen.Emit(OpCodes.Blt, eachLabel);
                gen.MarkLabel(emptyLabel);
                gen.Emit(OpCodes.Ldloc, resultBulider.LocalIndex);
                gen.Emit(OpCodes.Ret);
                del = method.CreateDelegate(typeof(ToTableList<T>));
                cache_del.TryAdd(tType, del);
            }
            return (del as ToTableList<T>)(table, suffix);
        }


        private static DataRow GetRow(DataTable table, int index)
        {
            return table.Rows[index];
        }

        private static TValue GetRowValue<TValue>(DataRow row, string key)
        {
            return GetItem<TValue>(key, row);
        }

        private static T GetItem<T>(string columnName, DataRow row)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return default(T);
            }
            object obj = row[columnName];
            if (obj is T)
            {
                return (T)obj;
            }
            object r_obj = default(T);
            if (row.IsNull(columnName))
            {
                return default(T);
            }
            else if (obj == null)
            {
                return (T)r_obj;
            }
            if (obj is string)
            {
                string str = obj.ToString();
                if (typeof(T) == typeof(Guid))
                {
                    Guid r = Guid.Empty;
                    if (Guid.TryParse(str, out r))
                    {
                        r_obj = r;
                    }
                    else
                    {
                        r_obj = Guid.Empty;
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    DateTime r = DateTime.MinValue;
                    if (DateTime.TryParse(str, out r))
                    {
                        r_obj = r;
                    }
                    else
                    {
                        r_obj = DateTime.MinValue;
                    }
                }
                else if (typeof(T) == typeof(TimeSpan))
                {
                    TimeSpan r = TimeSpan.MinValue;
                    if (TimeSpan.TryParse(str, out r))
                    {
                        r_obj = r;
                    }
                    else
                    {
                        r_obj = TimeSpan.MinValue;
                    }
                }
                else
                {
                    try
                    {
                        r_obj = Convert.ChangeType(str, typeof(T));
                    }
                    catch (Exception)
                    {
                        r_obj = default(T);
                    }
                }
            }
            else if (typeof(T).IsEnum)
            {
                string str = obj.ToString();
                T r = default(T);
                if (EnumTryParse<T>(str, out r))
                {
                    r_obj = r;
                }
                else
                {
                    r_obj = default(T);
                }
            }
            else
            {
                r_obj = Convert.ChangeType(obj, typeof(T));
            }
            return (T)r_obj;
        }

        private static bool EnumTryParse<T>(string value, out T result)
        {
            try
            {
                result = (T)Enum.Parse(typeof(T), value, true);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }
    }
}
