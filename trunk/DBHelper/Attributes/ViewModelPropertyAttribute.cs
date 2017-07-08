using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ViewModelPropertyAttribute : Attribute
    {
        public ViewModelPropertyAttribute()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private string _DBConnectionName;
        /// <summary>
        /// This property is used for Multi-DB support
        /// </summary>
        public string DBConnectionName
        {
            get { return _DBConnectionName; }
            set { _DBConnectionName = value; }
        }


        private string _SortExpression;
        public string SortExpression
        {
            get { return _SortExpression; }
            set 
            { 
                _SortExpression = value;
                if (!string.IsNullOrEmpty(DBConnectionName))
                {
                    _SortExpression = getSecurityDBName(DBConnectionName) + "." + value;
                }
            }
        }

        private static string getSecurityDBName(string DBConnectionName)
        {
            string dbName = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[DBConnectionName].ConnectionString;
            string[] arr = conStr.Split(';');
            foreach (string str in arr)
            {
                if (str.ToLower().Contains("initial catalog"))
                {
                    dbName = str.Split('=')[1];
                    break;
                }
            }

            return dbName;
        }

        private bool _IsDesc;
        public bool IsDesc
        {
            get { return _IsDesc; }
            set { _IsDesc = value; }
        }
    }
}