using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using System.Data.Objects.DataClasses;  //System.Data.Entity.dll

namespace Base
{
    public static class TypeConverter
    {
        public static Dictionary<string, object> AnonymousToDictionary(object value)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (value != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(value))
                {
                    object obj2 = descriptor.GetValue(value);
                    dic.Add(descriptor.Name, obj2);
                }
            }
            return dic;
        }

        //public static T OrderedDictionaryToObject<T>(IOrderedDictionary KeyValues)
        //{
        //    T obj = DBHelper.Reflect.GetInstance<T>();
        //    foreach (System.Collections.DictionaryEntry keyval in KeyValues)
        //    {

        //        obj.GetType().GetProperty(keyval.Key.ToString()).SetValue(obj, keyval.Value, null);
        //    }
        //    return obj;
        //}
     
        public static DataTable ToDataTable<T>(this IList<T> data, bool IsEntity=true)
        {
            IEnumerable<PropertyInfo> target_properties = null ;
            if(IsEntity)
                target_properties = from p in typeof(T).GetProperties()
                                                          where (from a in p.GetCustomAttributes(false)
                                                                 where a is EdmScalarPropertyAttribute
                                                                 select true).FirstOrDefault()
                                                          select p;
            else
                target_properties = from p in typeof(T).GetProperties() select p;
            //PropertyDescriptorCollection props =
            //    TypeDescriptor.GetProperties(typeof(T));
         
            DataTable table = new DataTable();
            foreach (PropertyInfo pi in target_properties)
            {
                if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType));
                else
                    table.Columns.Add(pi.Name, pi.PropertyType);
            }
            //for (int i = 0; i < props.Count; i++)
            //{                
            //    PropertyDescriptor prop = props[i];
            //    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType));
            //    else
            //        table.Columns.Add(prop.Name, prop.PropertyType);
            //}
            object[] values = new object[target_properties.Count()];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] =target_properties.ElementAt(i).GetValue(item,null);
                }
                table.Rows.Add(values);
            }
            return table;
        }

    }
}
