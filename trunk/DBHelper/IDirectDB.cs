using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace DBHelper
{
    /// <summary>
    /// Summary description for BaseDirectDB
    /// </summary>
    public interface IDirectDB
    {
        string ExecuteScalar(CommandType cType, string sqlStr, params SqlParameter[] parameters);
        int ExecuteNonQuery(CommandType cType, string sqlStr, params SqlParameter[] parameters);
        int SQL_Log(CommandType cType, string sqlStr, params SqlParameter[] parameters);        
        DataTable ExecuteQuery(CommandType cType, string sqlStr, params SqlParameter[] parameters);
        void Close();
    }
}