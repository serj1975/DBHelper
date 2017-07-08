using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.IO;
using LOG;

namespace DBHelper.SQLServer
{
    public class DirectDB : IDirectDB, IDisposable
    {
        public virtual void OnCreated()
        {
        }

        public void Close()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public DirectDB(string conStr,string loggerPath = "")
        {
            if (conStr.Contains("=") && conStr.Contains(";"))
                _connStr = conStr;
            else
                _connStr = System.Configuration.ConfigurationManager.ConnectionStrings[conStr].ConnectionString;
            if(!string.IsNullOrEmpty(loggerPath))
                SimpleLogger.PATH = loggerPath;
        }
       
        public DirectDB()
        {
            OnCreated();
            if (_conn == null) throw new Exception("Connection string is null or not defined");
        }

        //~DirectDB()
        //{
        //    if (_conn == null) return;
        //    if (_conn.State == System.Data.ConnectionState.Open)
        //    {
        //        _conn.Close();
        //    }

        //}   
        private string _connStr;
        public string ConnectionString
        {
            get { return _connStr; }
            set { _connStr = value; }
        }
        private SqlConnection _conn;
        private SqlConnection Connection
        {
            get
            {
                if (_conn == null)
                {
                    _conn = new SqlConnection(_connStr);
                }
                if (_conn.State == System.Data.ConnectionState.Broken ||
                    _conn.State == System.Data.ConnectionState.Closed)
                {
                    _conn.Open();
                }
                return _conn;
            }
        }

        private string _baseLogPath;
        public string BaseLogPath
        {
            set { _baseLogPath = value; }
            get
            {
                if (!string.IsNullOrEmpty(_baseLogPath)) return _baseLogPath;
                else throw new Exception("BaseLogPath is not initialized");
            }
        }
     

        public string ExecuteScalar(CommandType cType, string sqlStr,params SqlParameter[] parameters)
        {
            SimpleLogger.LogSQL(sqlStr, parameters);
            try
            {
                using (SqlCommand cmd = new SqlCommand(sqlStr, Connection))
                {
                    cmd.CommandType = cType;
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                    object obj = cmd.ExecuteScalar();
                    if (obj != null) return obj.ToString();
                    else return null;
                }
            }
            catch (Exception e)
            {
                SimpleLogger.LogException("ExecuteScalar", sqlStr + "|" + e.Message, e);
                throw e;
            }      
        }
    
        public DataTable ExecuteQuery(CommandType cType, string sqlStr, params SqlParameter[] parameters)
        {
            SimpleLogger.LogSQL(sqlStr, parameters);
            DataTable dt = new DataTable();
            try
            {
                using (SqlDataAdapter ada = new SqlDataAdapter(sqlStr, Connection))
                {
                    ada.SelectCommand.CommandType = cType;
                    foreach (SqlParameter p in parameters)
                    {
                        ada.SelectCommand.Parameters.Add(p);
                    }
                    ada.Fill(dt);
                }
            }
            catch (Exception e)
            {
                SimpleLogger.LogException("ExecuteQuery", sqlStr + "|" + e.Message, e);
                throw e;
            }      

            return dt;
        }

        public DataSet ExecuteQueryDataSet(CommandType cType, string sqlStr, params SqlParameter[] parameters)
        {
            SimpleLogger.LogSQL(sqlStr, parameters);
            DataSet ds = new DataSet();
            try
            {
                using (SqlDataAdapter ada = new SqlDataAdapter(sqlStr, Connection))
                {
                    ada.SelectCommand.CommandType = cType;
                    foreach (SqlParameter p in parameters)
                    {
                        ada.SelectCommand.Parameters.Add(p);
                    }
                    ada.Fill(ds);
                }
            }
            catch (Exception e)
            {
                SimpleLogger.LogException("ExecuteQuery", sqlStr + "|" + e.Message, e);
                throw e;
            }

            return ds;
        }

        public int ExecuteNonQuery(CommandType cType, string sqlStr, params SqlParameter[] parameters)
        {
            SimpleLogger.LogSQL(sqlStr, parameters);
            try
            {
                using (SqlCommand cmd = new SqlCommand(sqlStr, Connection))
                {
                    cmd.CommandType = cType;
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                SimpleLogger.LogException("ExecuteNonQuery", sqlStr + "|" + e.Message, e);
                throw e;
            }
       
        }

        public object GetReturnValue(CommandType cType, string sqlStr, params SqlParameter[] parameters)
        {
            SimpleLogger.LogSQL(sqlStr, parameters);
            try
            {
                SqlParameter return_value = new SqlParameter("return_value",null);
                return_value.Direction = ParameterDirection.ReturnValue;
                using (SqlCommand cmd = new SqlCommand(sqlStr, Connection))
                {
                    cmd.CommandType = cType;
                    cmd.Parameters.Add(return_value);
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                    cmd.ExecuteNonQuery();
                    return cmd.Parameters["return_value"].Value;
                }
            }
            catch (Exception e)
            {
                SimpleLogger.LogException("GetReturnValue", sqlStr + "|" + e.Message, e);
                throw e;
            }

        }

        public int SQL_Log(CommandType cType, string sqlStr, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sqlStr, Connection))
                {
                    cmd.CommandType = cType;
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
