using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Base.Extension.StringExtension;

namespace DBHelper.SQLServer
{
    public class DataDictionary
    {
        DirectDB DB;
        public DataDictionary(string connectionStringName)
        {
            DB = new DirectDB(System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
        }
        public DataTable GetTableDescription(string TableName)
        {
            string sqlStr = @" 
SELECT     obj.name, ext.value
FROM         sys.sysobjects AS obj INNER JOIN
                      sys.extended_properties AS ext ON obj.id = 

ext.major_id
WHERE     (obj.name LIKE @name) AND (ext.minor_id = 0)";

//            string sqlStr = @"SELECT  tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME AS TableName, prop.value AS TableDescription
//FROM            INFORMATION_SCHEMA.TABLES AS tbl LEFT OUTER JOIN
//                         sys.extended_properties AS prop ON prop.major_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND prop.minor_id = 0 AND 
//                         prop.name = 'MS_Description'
//WHERE        (tbl.TABLE_TYPE = 'base table') AND (obj.name LIKE @name) ";
            SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
            p.Value = TableName;
            return DB.ExecuteQuery(CommandType.Text,sqlStr, p);
         
        }

        public string GetTableDescriptionBySchema(string TableName)
        {
            string sqlStr = @"SELECT  tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME AS TableName, prop.value AS TableDescription
FROM            INFORMATION_SCHEMA.TABLES AS tbl LEFT OUTER JOIN
                         sys.extended_properties AS prop ON prop.major_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND prop.minor_id = 0 AND 
                         prop.name = 'MS_Description'
WHERE        (tbl.TABLE_TYPE = 'base table')";
            //SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
            //p.Value = TableName;

            DataRow[] dr= DB.ExecuteQuery(CommandType.Text, sqlStr).Select("TableName = '" + TableName + "'");
            string retStr = "\"" + TableName + "\"";
            if (dr.Count() > 0)
            {
                string str = dr[0]["TableDescription"].ToString().Trim();
                if (!string.IsNullOrEmpty(str)) retStr = "\"" + str + "\""; 
            }
            return retStr;

        }

        public DataTable GetFieldsDescription(string TableName)
        {        
                string sqlStr =    @"SELECT     col.name, ext.value
FROM         sys.sysobjects AS obj INNER JOIN
                      sys.syscolumns AS col ON col.id = obj.id INNER JOIN
                      sys.extended_properties AS ext ON obj.id = ext.major_id AND ext.minor_id = col.colid
WHERE     (obj.name LIKE @name)";
                SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
                p.Value = TableName;
                return DB.ExecuteQuery(CommandType.Text, sqlStr, p);  
        }

        public string GetFieldsDescriptionBySchema(string TableName, string colname)
        {
            string sqlStr = @"SELECT  tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME AS TableName, tbl.TABLE_NAME AS orgName, prop.value AS TableDescription
FROM            INFORMATION_SCHEMA.TABLES AS tbl LEFT OUTER JOIN
                         sys.extended_properties AS prop ON prop.major_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND prop.minor_id = 0 AND 
                         prop.name = 'MS_Description'
WHERE        (tbl.TABLE_TYPE = 'base table')";
            //SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
            //p.Value = TableName;

            DataRow[] dr = DB.ExecuteQuery(CommandType.Text, sqlStr).Select("TableName = '" + TableName + "'");
            if (dr.Count() > 0)
                TableName = dr[0]["orgName"].ToString();
//            ////
           
           sqlStr =
@"
SELECT     col.name, ext.value
FROM         sys.sysobjects AS obj INNER JOIN
                      sys.syscolumns AS col ON col.id = obj.id INNER JOIN
                      sys.extended_properties AS ext ON obj.id = ext.major_id AND ext.minor_id = col.colid
WHERE     (obj.name LIKE '@name' and col.name LIKE '@col')";
           sqlStr = sqlStr.NamedStringFormatSQL(new
           {
               name = TableName,
               col = colname
           });
            //SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
            //p.Value = colname;
            DataTable dt =  DB.ExecuteQuery(CommandType.Text, sqlStr);
            //DataRow[] drc =dt.Select("name = '" + colname  +"'");
            if (dt.Rows.Count > 0)
                return "\"" + dt.Rows[0][1].ToString() + "\"";
            else
                return "\"" + colname + "\"";
        }

        public string GetConstraintDescription(string ConstraintName)
        {
            string sqlStr = @"
select value from sys.objects obj
inner join sys.extended_properties ext
on obj.object_id = ext.major_id
where obj.name = @name
";
            SqlParameter p = new SqlParameter("name", SqlDbType.VarChar);
            p.Value = ConstraintName;
            string msg = DB.ExecuteScalar(CommandType.Text, sqlStr, p);
            if (msg != null) return msg;
            else return ConstraintName;
        }
    }
}
