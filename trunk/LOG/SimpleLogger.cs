using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using System.Web;
using System.Data.Common;
using DBHelper;
using System.Web.Security;

namespace LOG
{
    public enum LoggerLevel
    {
        ALL = 0,
        TRACE = 1,
        DEBUG = 2,
        INFO = 3,
        WARN = 4,
        ERROR = 5,
        FATAL = 6,
        OFF = 7
    }
    public enum LoggerProviersType
    {
        FILE = 0,
        DB = 1
    }

    public struct LogStru
    {
        public string UserName { get; set; }
        public string DateF { get; set; }
        public string Time { get; set; }
        public string LogTypeIdentifier { get; set; }
        public string LogMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public static class SimpleLogger
    {

        public static string LogSeparator = "\r\n************************************************************************\r\n";
        public static LoggerProviersType LogProvider = LoggerProviersType.FILE;
        public static LoggerLevel loggerLevel = LoggerLevel.OFF;
        private static string config_path;
        private static string _path = "";
        public static string PATH
        {
            get
            {
                return _path;
            }
            set
            {
                value = setPath(value);
            }
        }
        private static string _DBConnection;
        public static string DBConnection
        {
            get
            {
                if (string.IsNullOrEmpty(_DBConnection))
                    throw new Exception("Your LogProvider is Database, but DBConnection was not provided thourgh [LogCon] or SimpleLogger class directly");
                return _DBConnection;
            }
            set { _DBConnection = value; }
        }

        private static string _DBConnectionName;
        public static string DBConnectionName
        {
            get { return _DBConnectionName; }
            set
            {
                _DBConnectionName = value;
                if (System.Configuration.ConfigurationManager.ConnectionStrings[_DBConnectionName] != null)
                    _DBConnection = System.Configuration.ConfigurationManager.ConnectionStrings[_DBConnectionName].ConnectionString;
            }
        }

        static SimpleLogger()
        {
            string _provider = System.Configuration.ConfigurationManager.AppSettings["SimpleLoggerProvider"];
            if (!string.IsNullOrEmpty(_provider))
            {
                if (_provider.ToLower() == "db")
                {
                    LogProvider = LoggerProviersType.DB;
                    if (string.IsNullOrEmpty(DBConnectionName))
                    {
                        DBConnectionName = System.Configuration.ConfigurationManager.AppSettings["SimpleLoggerDBConnection"];
                        if (string.IsNullOrEmpty(_DBConnectionName)) _DBConnectionName = "LogCon";
                    }
                }
            }
            string _log_level = System.Configuration.ConfigurationManager.AppSettings["SimpleLoggerLevel"];
            if (!string.IsNullOrEmpty(_log_level))
            {
                loggerLevel = (LoggerLevel)Enum.Parse(typeof(LoggerLevel), _log_level, true);
            }

            config_path = System.Configuration.ConfigurationManager.AppSettings["SimpleLoggerPath"];
            if (string.IsNullOrEmpty(config_path)) config_path = "";
            _path = setPath("");

        }


        private static void WriteLog(LogStru stru)
        {
            if (LogProvider == LoggerProviersType.FILE)
            {
                File.AppendAllText(PATH, "\r\n" + string.Format("User:{0}   Date:{1}  Time:{2}  LogTypeIdentifier:{3}\nLogMessage:{4}{5}", stru.UserName, stru.DateF, stru.Time, stru.LogTypeIdentifier, stru.LogMessage,LogSeparator));
            }
            if (LogProvider == LoggerProviersType.DB)
            {
                string sqlTemplate = " insert into tblLog values(N'{0}','{1}','{2}',N'{3}',N'{4}',N'{5}') ";
                StringBuilder sqlStr = new StringBuilder();
                sqlStr.AppendFormat(sqlTemplate, stru.UserName.Replace("'", "\""), stru.DateF, stru.Time, stru.LogTypeIdentifier.Replace("'", "\""), stru.LogMessage.Replace("'", "\""), stru.StackTrace.Replace("'", "\""));
                IDirectDB DB = new DBHelper.SQLServer.DirectDB(DBConnection);
                DB.SQL_Log(System.Data.CommandType.Text, sqlStr.ToString());
                DB.Close();


            }
        }

        private static void WriteExceptionLog(List<LogStru> struList)
        {
            if (LogProvider == LoggerProviersType.FILE)
            {
                StringBuilder str = new StringBuilder();
                bool first = true;
                foreach (LogStru stru in struList)
                {
                    string Message = stru.LogMessage;
                    string pre = "";
                    if (!first)
                    {
                        pre = "\t";
                        Message = "----------------------InnerException---------------------------";
                    }

                    str.AppendLine("");
                    str.AppendLine(pre + "User                :  " + stru.UserName);
                    str.AppendLine(pre + "Time                :  " + stru.Time);
                    str.AppendLine(pre + "LogTypeIdentifier   :  " + stru.LogTypeIdentifier);
                    str.AppendLine(pre + "LogMessage          :  " + Message);
                    str.AppendLine(pre + "StackTrace          :\r\n" + stru.StackTrace);
                    str.AppendLine("");
                    first = false;
                }
                File.AppendAllText(PATH, str.ToString()+LogSeparator);
            }
            if (LogProvider == LoggerProviersType.DB)
            {
                string sqlTemplate = " insert into tblLog values(N'{0}','{1}','{2}',N'{3}',N'{4}',N'{5}') ";
                StringBuilder sqlStr = new StringBuilder();
                foreach (LogStru stru in struList)
                {
                    sqlStr.AppendFormat(sqlTemplate, stru.UserName, stru.DateF, stru.Time, stru.LogTypeIdentifier, stru.LogMessage, stru.StackTrace);
                }

                IDirectDB DB = new DBHelper.SQLServer.DirectDB(DBConnection);
                DB.SQL_Log(System.Data.CommandType.Text, sqlStr.ToString());
                DB.Close();

            }
        }

        private static string setPath(string value)
        {
            string path = "";
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(config_path))
                    value = config_path;
                else
                    value = "";
            }
            if (HttpContext.Current != null)
                value = HttpContext.Current.Server.MapPath("~/").Replace("\\", "/") + value.Replace("\\", "/");
            if (value.Contains('.'))
            {
                path = value;
            }
            else
            {
                if (!value.EndsWith("/")) value += "/";
                path = value + "SimpleLogger_" + PersianDate(DateTime.Now).Replace("/", "") + ".log";

            }
            return path;
        }


        public static void Log(string msg, string LogTypeIdentifier = "Log", LoggerLevel level = LoggerLevel.DEBUG)
        {
            if (SimpleLogger.loggerLevel > level) return;

            LogStru stru = new LogStru();
            stru.UserName = GetLoggerUser();
            stru.DateF = PersianDate(DateTime.Now);
            stru.Time = GetTime();
            stru.LogMessage = msg;
            stru.LogTypeIdentifier = LogTypeIdentifier;
            stru.StackTrace = "";
            WriteLog(stru);
        }

        private static string GetTime()
        {
            DateTime dt = DateTime.Now;
            string time = "{0}:{1}:{2}.{3}";
            time = string.Format(time, dt.Hour.ToString(), dt.Minute.ToString(), dt.Second.ToString(), dt.Millisecond.ToString());
            return time;
        }

        public static void LogSQL(string sqlStr, params DbParameter[] parameters)
        {
            if (SimpleLogger.loggerLevel > LoggerLevel.TRACE) return;

            if (parameters != null)
            {
                foreach (DbParameter par in parameters)
                {
                    sqlStr = sqlStr.Replace("@" + par.ParameterName, par.IsNullToStr(""));
                }
            }
            LogStru stru = new LogStru();
            stru.UserName = GetLoggerUser();
            stru.DateF = PersianDate(DateTime.Now);
            stru.Time = GetTime();
            stru.LogMessage = sqlStr;
            stru.LogTypeIdentifier = "LogSQL";
            stru.StackTrace = "";
            WriteLog(stru);

        }
        public static void LogException(string LogTypeIdentifier, string Description, Exception e, LoggerLevel level = LoggerLevel.ERROR)
        {
            if (SimpleLogger.loggerLevel > level) return;
            string UserName = GetLoggerUser();

            List<LogStru> list = GetLogStruList(e,LogTypeIdentifier,Description);
            WriteExceptionLog(list);
        }

        public static List<LogStru> GetLogStruList(Exception e,string LogTypeIdentifier="", string Description="")
        {
            List<LogStru> list = new List<LogStru>();
            Exception e1 = e;
            while (e1 != null)
            {
                LogStru stru = new LogStru();
                stru.UserName = GetLoggerUser();
                stru.DateF = PersianDate(DateTime.Now);
                stru.Time = GetTime();
                stru.LogMessage = string.Format("{0} \n| {1}", Description,e1.Message);
                stru.LogTypeIdentifier = LogTypeIdentifier + e1.Source;
                stru.StackTrace = e1.StackTrace;
                list.Add(stru);
                e1 = e1.InnerException;
            }
            return list;
        }

        private static string GetLoggerUser()
        {
            string UserName = "Anonymous";
            bool _membershipAuth = Base.Config.WebConfigManager.GetAuthMode() == System.Web.Configuration.AuthenticationMode.Forms;
            if (_membershipAuth)
            {
                if (Membership.GetUser() != null)
                    UserName = Membership.GetUser().UserName;
            }
            if (!_membershipAuth)  // Windows Authentication
            {
                UserName = HttpContext.Current.Request.LogonUserIdentity.Name;
                if (UserName.Contains('\\')) UserName = UserName.Split('\\')[1];
                if (UserName.Contains('@')) UserName = UserName.Split('@')[0];
            }
            //if (MSS.Config.WebConfigManager.GetAuthMode() == System.Web.Configuration.AuthenticationMode.None)
            //    UserName = "admin";
            return UserName;
        }

        public static string PersianDate(DateTime dat)
        {
            string strs = "";
            try
            {
                DateTime dt = dat;
                PersianCalendar pc = new PersianCalendar();
                int[] x = new int[6];
                x[0] = pc.GetYear(dt);
                x[1] = pc.GetMonth(dt);
                x[2] = pc.GetDayOfMonth(dt);
                x[3] = pc.GetHour(dt);
                x[4] = pc.GetMinute(dt);
                x[5] = pc.GetSecond(dt);
                string[] strArr = new string[3];
                strArr[0] = x[0].ToString();
                strArr[1] = x[1].ToString();
                strArr[2] = x[2].ToString();
                if (strArr[1].Length < 2) strArr[1] = "0" + strArr[1];
                if (strArr[2].Length < 2) strArr[2] = "0" + strArr[2];
                strs = strArr[0] + "/" + strArr[1] + "/" + strArr[2];
                //x[0].ToString() + "/" + x[1].ToString() + "/" + x[2].ToString(); //+ " " + x[3].ToString() + ":" + x[4].ToString() + ":" + x[5].ToString();
            }
            catch
            {
            }
            return strs;
        }

        public static string IsNullToStr(this object obj, string Default = "")
        {
            if (obj != null)
                return obj.ToString();
            else
                return Default;
        }
    }
}
