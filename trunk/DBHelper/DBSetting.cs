using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace DBHelper
{

    /// <summary>
    /// Summary description for DBSetting
    /// </summary>
    public static class DBSetting
    {
        static string _DBConnectionName = System.Configuration.ConfigurationManager.AppSettings.Get("DBConnectionName");
        static string _ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[_DBConnectionName].ConnectionString;
        static DBHelper.DatabaseType _DBType = (DBHelper.DatabaseType)Enum.Parse(typeof(DatabaseType), System.Configuration.ConfigurationManager.AppSettings.Get("DBType"), true);
        public static string ConnectionString
        {
            get { return _ConnectionString; }
        }
        public static DBHelper.DatabaseType DBType
        {
            get { return _DBType; }
        }
    }
}