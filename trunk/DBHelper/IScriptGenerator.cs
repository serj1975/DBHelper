using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace DBHelper
{
    /// <summary>
    /// Summary description for BaseScriptGenerator
    /// </summary>
    public interface IScriptGenerator
    {
     
        string Insert(string tableName, string keyName, object instance, bool autoIncrement);
        string Update(string tableName, string keyName, object instance);
        string Delete(string tableName, string keyName, object IdValue);
        string List(string tableName,string fields, string default_sortExpression, int startRowIndex, int maximumRows, string sortExpression, string filterExpression);
        string Get(string tableName, string keyName, object instance);
        string Count(string tableName, string filterExpression);
    }
}