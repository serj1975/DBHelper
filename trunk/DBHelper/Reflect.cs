using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using System.Text;
using DBHelper.Attributes;

namespace DBHelper
{
    /// <summary>
    /// Summary description for Reflect
    /// </summary>
    public static class Reflect
    {
        public static bool DataRow2Object(object instance, DataRow dr,bool skipNonExistField=false)
        {
            Type t1 = instance.GetType();
            foreach (PropertyInfo pi in t1.GetProperties())
            {
                //DBFieldMetadataAttribute attr = (DBFieldMetadataAttribute)System.Attribute.GetCustomAttribute(pi, typeof(DBFieldMetadataAttribute));
                //if (attr.NonTableField) continue;
                if (!pi.CanWrite) continue;
                if (skipNonExistField && !dr.Table.Columns.Contains(pi.Name)) continue;
                if (dr[pi.Name] == DBNull.Value) continue;

                try
                {
                    object fieldValue = DBType2ObjType(dr[pi.Name], pi.PropertyType);
                    pi.SetValue(instance, fieldValue, null);
                }
                catch (Exception e)
                {
                    throw new Exception("'" + pi.Name + "' cannot be converted from DBType to ObjectType \r\n" + e.Message);
                }

            }
            return true;
        }

        private static object DBType2ObjType(object dbValue, Type targetType)
        {
            object retVal = dbValue;
            if (dbValue.GetType() == typeof(int) && targetType == typeof(bool))
                retVal = (int)dbValue == 0 ? false : true;
            return retVal;

        }

        public static IList<T> DataTable2ObjectList<T>(DataTable dt, bool skipNonExistField = false)
        {
            IList<T> list = new List<T>();
            if (dt == null || dt.Rows.Count == 0) return list;
            foreach (DataRow r in dt.Rows)
            {
                T item = GetInstance<T>();
                DataRow2Object(item, r, skipNonExistField);
                list.Add(item);
            }
            return list;
        }

        public static T GetInstance<T>(string type)
        {
            return (T)Activator.CreateInstance(Type.GetType(type));
        }

        public static T GetInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static DBEntityMetadataAttribute GetEntityMetadata(Type t)
        {
            return (DBEntityMetadataAttribute)t.GetCustomAttributes(typeof(DBEntityMetadataAttribute), false).FirstOrDefault();
        }

        public static IList<DBFieldMetadata> GetFieldsMetadata(Type t)
        {
            List<DBFieldMetadata> fieldAttribs = new List<DBFieldMetadata>();
            PropertyInfo[] pis = t.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                DBFieldMetadataAttribute attr = (DBFieldMetadataAttribute)System.Attribute.GetCustomAttribute(pi, typeof(DBFieldMetadataAttribute));
                //fieldAttribs.Add(pi.Name, attr);
                fieldAttribs.Add(new DBFieldMetadata() { Key = pi.Name, Attrib = attr });
            }
            return fieldAttribs.OrderBy(a => a.Attrib.Ordering).ToList();

        }

        public static ViewModelPropertyAttribute GetViewModelProperty<T>(T ViewModel, string property)
        {
            bool IsDesc = false;
            if (property.Contains("DESC"))
            {
                IsDesc = true;
                property = property.Replace("DESC", "").Trim();
            }
            PropertyInfo pi = ViewModel.GetType().GetProperty(property);
            ViewModelPropertyAttribute attr = (ViewModelPropertyAttribute)System.Attribute.GetCustomAttribute(pi, typeof(ViewModelPropertyAttribute));
            if (attr == null)
            {
                attr = new ViewModelPropertyAttribute();
                attr.SortExpression = pi.Name;
            }
            attr.IsDesc = IsDesc;
            return attr;
        }

        /// <summary>
        /// Reflect through Persist object and get Properties
        /// NonTableField excluded
        /// </summary>
        /// <param name="t"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetDBTableFields(Type t, object instance)
        {
            Dictionary<string, object> Fields = new Dictionary<string, object>();
            PropertyInfo[] pis = instance.GetType().GetProperties();
            foreach (PropertyInfo p in pis)
            {
                DBFieldMetadataAttribute attr = (DBFieldMetadataAttribute)System.Attribute.GetCustomAttribute(p, typeof(DBFieldMetadataAttribute));
                if (attr.NonTableField) continue;
                Fields.Add(p.Name, p.GetValue(instance, null));
            }
            return Fields;
        }

        public static string GetQualifiedDBTableFieldsCommaSeparated(Type t)
        {
            string tableName = Reflect.GetEntityMetadata(t).TableName;
            StringBuilder Fields = new StringBuilder("");
            PropertyInfo[] pis = t.GetProperties();
            foreach (PropertyInfo p in pis)
            {
                DBFieldMetadataAttribute attr = (DBFieldMetadataAttribute)System.Attribute.GetCustomAttribute(p, typeof(DBFieldMetadataAttribute));
                if (attr.NonTableField)
                    Fields.AppendFormat("{0},", p.Name);
                else
                    Fields.AppendFormat("{0}.{1},", tableName, p.Name);
            }
            return Fields.ToString().TrimEnd(',');
        }

        public static string GetQualifiedDBTableField(Type t, string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            string tableName = Reflect.GetEntityMetadata(t).TableName;
            string field = "";
            PropertyInfo p = t.GetProperty(name);
            DBFieldMetadataAttribute attr = (DBFieldMetadataAttribute)System.Attribute.GetCustomAttribute(p, typeof(DBFieldMetadataAttribute));
            if (attr.NonTableField)
                field = p.Name;
            else
                field += tableName + "." + p.Name;
            return field;
        }

    }
}