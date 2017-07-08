using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DBHelper.SQLServer
{
    class ScriptGenerator : IScriptGenerator
    {
        public string Insert(string tableName, string keyName, object instance, bool autoIncrement)
        {
            Dictionary<string, object> FieldsKeyValue = Reflect.GetDBTableFields(instance.GetType(), instance);
            string flds = "(";
            string values = "(";
            foreach (var fldKV in FieldsKeyValue)
            {
                if (autoIncrement && fldKV.Key == keyName)
                {
                    continue;
                }

                flds += fldKV.Key + ",";
                if (fldKV.Value.GetType() == typeof(string))
                    values += "N'" + fldKV.Value.ToString() + "',";

                else if (fldKV.Value.GetType() == typeof(bool))
                {
                    int bit_value = (bool)fldKV.Value ? 1 : 0;
                    values += bit_value + ",";
                }
                else
                    values += fldKV.Value.ToString() + ",";

            }
            flds = flds.TrimEnd(',') + ")";
            values = values.TrimEnd(',') + ")";
            string sqlStr = "INSERT INTO " + tableName + "\n" +
                flds + "\nVALUES" + values;
            sqlStr += ";\nSELECT @@Identity";
            return sqlStr;
        }

        public string Update(string tableName, string keyName, object instance)
        {
            Dictionary<string, object> FieldsKeyValue = Reflect.GetDBTableFields(instance.GetType(), instance);
            string flds = "";
            object idValue = null;
            foreach (var fldKV in FieldsKeyValue)
            {
                if (fldKV.Key == keyName)
                {
                    idValue = fldKV.Value.ToString();
                    continue;
                }
                if (fldKV.Value.GetType() == typeof(string))
                    flds += fldKV.Key + "=N'" + fldKV.Value.ToString() + "',";
                else if (fldKV.Value.GetType() == typeof(bool))
                {
                    int bit_value = (bool)fldKV.Value ? 1 : 0;
                    flds += fldKV.Key + "=" + bit_value + ",";
                }
                else
                    flds += fldKV.Key + "=" + fldKV.Value.ToString() + ",";
            }

            flds = flds.TrimEnd(',');
            string sqlStr = "UPDATE " + tableName +
              " SET " + flds + " WHERE " + keyName + "=" + idValue.ToString();
            return sqlStr;         
        }

        public string Delete(string tableName, string keyName, object KeyValue)
        {
            string sql = @"DELETE FROM  [{tableName}] WHERE {keyName} = {KeyValue}";
            sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(sql, new { tableName, keyName, KeyValue });
            return sql;
        }

        public string List(string tableName,string fields, string default_sortExpression, int startRowIndex, int maximumRows, string sortExpression, string filterExpression)
        {
            if (string.IsNullOrWhiteSpace(sortExpression)) sortExpression = default_sortExpression;
            if (string.IsNullOrWhiteSpace(filterExpression)) filterExpression = "1=1";
            if (string.IsNullOrEmpty(fields)) fields = "*";
            string sql =
 @"SELECT * FROM  
(SELECT {fields},ROW_NUMBER() OVER(ORDER BY {sortExpression}) as RowNum
FROM {tableName}
) as DerivedTableName
WHERE RowNum BETWEEN {startRowIndex} AND ({startRowIndex} + {maximumRows}) - 1 AND {filterExpression}";
            sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(sql, new { tableName, fields, startRowIndex, maximumRows, sortExpression, filterExpression });
            return sql;
        }

        public string Get(string tableName, string keyName,object IdValue)
        {
            string sql = @"SELECT * FROM  [{tableName}] WHERE {keyName} = {IdValue}";
            sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(sql, new { tableName, keyName, IdValue });
            return sql;
        }

        public string Count(string tableName, string filterExpression)
        {
            if (string.IsNullOrWhiteSpace(filterExpression)) filterExpression = "1=1";
            string sql = "SELECT COUNT(*)  FROM {tableName} WHERE {filterExpression}";
            sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(sql, new { tableName, filterExpression });
            return sql;
        }      
    }
}
