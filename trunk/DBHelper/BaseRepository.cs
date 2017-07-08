using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using DBHelper;
using DBHelper.Attributes;

/// <summary>
/// Summary description for BaseRepository
/// </summary>
public class BaseRepository<T>
{
    public IDirectDB DB;
    public IScriptGenerator ScriptGenerator;
    //public BaseRepository()
    //{
    //    Init("AppCon");
    //}
    private string _tableName = "";
    private string _default_sortExpression = "";
    private string _IDColumn = "";
    private bool _autoIncrement = false;

    private IDirectDB InitDB(DatabaseType dbType, string ConnectionString)
    {
        IDirectDB db = null;
        switch (dbType)
        {
            //case DatabaseType.SQLite:
            //    db = new DBHelper.SQLite.DirectDB(ConnectionString);
            //    break;
            case DatabaseType.SQLServer:
                db = new DBHelper.SQLServer.DirectDB(ConnectionString);
                break;
        }
        return db;
    }
    private IScriptGenerator InitScriptGenerator(DatabaseType dbType)
    {
        IScriptGenerator scriptManager = null;
        switch (dbType)
        {
            //case DatabaseType.SQLite:
            //    scriptManager = new DBHelper.SQLite.ScriptGenerator();
            //    break;
            case DatabaseType.SQLServer:
                scriptManager = new DBHelper.SQLServer.ScriptGenerator();
                break;
        }
        return scriptManager;
    }
    public BaseRepository(DatabaseType dbType, string ConnectionString)
    {
        DB = InitDB(dbType, ConnectionString);
        ScriptGenerator = InitScriptGenerator(dbType);
        DBEntityMetadataAttribute attrib =  Reflect.GetEntityMetadata(typeof(T));
        _tableName = attrib.TableName;
        _default_sortExpression = attrib.DefaultSort;
        _IDColumn = attrib.KeyName;
        _autoIncrement = attrib.AutoIncrementKey;
    }
    //public BaseRepository(DatabaseType dbType, string ConnectionString, string tableName, string defaultSortExpression, string IDColumn, bool AutoIncrement)
    //{
    //    DB = InitDB(dbType, ConnectionString);
    //    ScriptGenerator = InitScriptGenerator(dbType);

    //    _tableName = tableName;
    //    _default_sortExpression = defaultSortExpression;
    //    _IDColumn = IDColumn;
    //    _autoIncrement = AutoIncrement;
    //}


    public virtual DataTable List(int startRowIndex, int maximumRows, string sortExpression, string filterExpression)
    {
        string sql = ScriptGenerator.List(_tableName,"", _default_sortExpression, startRowIndex, maximumRows, sortExpression, filterExpression);
        DataTable dt = DB.ExecuteQuery(CommandType.Text, sql);
        //Dictionary<string, FieldMetadataAttribute> fieldAttris = Reflect.GetFieldsMetaData(typeof(T));
        //foreach (DataColumn col in dt.Columns)
        //{
        //    if (fieldAttris.ContainsKey(col.ColumnName))
        //        col.Caption = fieldAttris[col.ColumnName].Label;
        //}
        return dt;

    }

    public virtual DataTable Joinedist(string joinExpression, int startRowIndex, int maximumRows, string sortExpression, string filterExpression)
    {
        string fields = Reflect.GetQualifiedDBTableFieldsCommaSeparated(typeof(T));
        if (sortExpression != "")
        {
            string sortField = sortExpression.Replace(" DESC", "");
            sortExpression = sortExpression.Replace(sortField, Reflect.GetQualifiedDBTableField(typeof(T), sortField));
        }
        string sqlStr = ScriptGenerator.List(joinExpression, fields, this._default_sortExpression, startRowIndex, maximumRows, sortExpression, filterExpression);

        return DB.ExecuteQuery(CommandType.Text, sqlStr);
    }

    public virtual int Count(string filterExpression)
    {
        string sql = ScriptGenerator.Count(_tableName, filterExpression);
        return int.Parse(DB.ExecuteScalar(CommandType.Text, sql));
    }

    public virtual T Get(object Id)
    {
        T instance = Reflect.GetInstance<T>();
        if (Id == null) return instance;
        string sql = ScriptGenerator.Get(_tableName, _IDColumn, Id);
        DataTable dt = DB.ExecuteQuery(CommandType.Text, sql);
        DataRow r = dt.Rows[0];

        
        Reflect.DataRow2Object(instance, r);
        return instance;
    }

    /// <summary>
    /// Updates a Persist Object
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Number of affected rows</returns>
    public virtual int Update(T entity)
    {
        string sqlStr = ScriptGenerator.Update(_tableName, _IDColumn, entity);
        return DB.ExecuteNonQuery(CommandType.Text, sqlStr);
    }

    /// <summary>
    /// Insert a Persist Object
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Id Column Value</returns>
    public virtual object Insert(T entity)
    {
        string sqlStr = ScriptGenerator.Insert(_tableName, _IDColumn, entity, _autoIncrement);
        return DB.ExecuteScalar(CommandType.Text, sqlStr);
    }

    public virtual int Delete(T entity)
    {
        object KeyValue = entity.GetType().GetProperty(_IDColumn).GetValue(entity,null);
        string sqlStr = ScriptGenerator.Delete(_tableName, _IDColumn, KeyValue);
        return DB.ExecuteNonQuery(CommandType.Text, sqlStr);
    }
}