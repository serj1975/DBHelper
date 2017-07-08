using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBHelper.Attributes
{
    /// <summary>
    /// Summary description for DBEntityMetadataAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DBEntityMetadataAttribute : Attribute
    {
        public string TableName { get; set; }
        public string KeyName { get; set; }
        public string DefaultSort { get; set; }
        public bool AutoIncrementKey { get; set; }
        public DBEntityMetadataAttribute()
        {


        }
    }
}