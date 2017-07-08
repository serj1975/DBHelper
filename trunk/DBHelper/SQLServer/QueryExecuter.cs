using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace DBHelper.SQLServer
{
    public class QueryExecuter
    {
        private string _connectionString;
        public QueryExecuter(string ConnectionName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
        }
        /// <summary>
        /// Get Sql query with paging parametes and sortExpression and return IList of T type given
        /// </summary>
        /// <typeparam name="T">Input Type</typeparam>
        /// <param name="sql">Sql Expression</param>
        /// <param name="startRowIndex">Current Page</param>
        /// <param name="maximumRows">Num Rows Displayed on Page</param>
        /// <param name="sortExpression">Sort Column Name and Direction(Asc/Desc)</param>
        /// <param name="filterExpression">Filter expression send from search UserControl</param>
        /// <param name="columns">Searchable fields list (Grid Colums sort expression_</param>
        /// <param name="noPaging">If true no paging done</param>
        /// <returns>returns IList of input type</returns>
        public IList<T> GetList<T>(string sql, int startRowIndex, int maximumRows, string sortExpression, string filterExpression, string columns, bool noPaging = false)
        {
            if (string.IsNullOrEmpty(sortExpression)) throw new Exception("sortExpression must be something");
            T ViewModel = Reflect.GetInstance<T>();
            DBHelper.Attributes.ViewModelPropertyAttribute attr = Reflect.GetViewModelProperty<T>(ViewModel, sortExpression);
            if (attr.IsDesc) sortExpression = attr.SortExpression + " DESC ";
            else sortExpression = attr.SortExpression;
            bool IsReplaced = false;
            if (!noPaging)
            {
                if (sql.Contains("FROM "))
                {
                    IsReplaced = true;
                    sql = sql.Replace("FROM ", ",ROW_NUMBER() OVER(ORDER BY {sortExpression}) as RowNum FROM ");
                }
                if (sql.Contains("from "))
                {
                    IsReplaced = true;
                    sql = sql.Replace("from ", ",ROW_NUMBER() OVER(ORDER BY {sortExpression}) as RowNum FROM ");
                }
                if (!IsReplaced)
                {
                    throw new Exception("Your sql expression should contains 'FROM  ' or 'from ' word (without quoates)");
                }
            }

            filterExpression = getFilter(filterExpression, columns, ViewModel);

            string baked_sql =
 @"SELECT * FROM  
({sqlExpression}  AND {filterExpression} 
) as DerivedName ";
            if (!(sql.ToLower().Contains("where\n") || sql.ToLower().Contains("where ")))
            {
                baked_sql =
 @"SELECT * FROM  
({sqlExpression}  WHERE {filterExpression} 
) as DerivedName ";
            }
            if (!noPaging)
                baked_sql += "\n WHERE RowNum BETWEEN {startRowIndex}+1 AND ({startRowIndex} + {maximumRows})";
            //WHERE RowNum BETWEEN {startRowIndex} AND ({startRowIndex} + {maximumRows}) - 1 ";
            baked_sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(baked_sql,
                new
                {
                    sqlExpression = sql,
                    startRowIndex = startRowIndex,
                    maximumRows = maximumRows
                ,
                    filterExpression = filterExpression,
                    sortExpression = sortExpression
                });
            DBHelper.SQLServer.DirectDB db = new DBHelper.SQLServer.DirectDB(_connectionString);
            DataTable dt = db.ExecuteQuery(CommandType.Text, baked_sql);
            db.Close();
            return Reflect.DataTable2ObjectList<T>(dt);

        }

        private string getFilter(string filterExpression, string columns, object ViewModel)
        {
            if (string.IsNullOrEmpty(filterExpression)) return "1=1";
            string filter1 = ApplyYeKe(filterExpression);
            string filter2 = ApplyYeKeReverse(filterExpression);
            if (filter1 == "" && filter2 == "")
                return makeFilter(filterExpression, columns, ViewModel);
            if (filter1 != "" && filter2 != "")
                return string.Format("({0} OR {1} OR {2})",makeFilter(filterExpression, columns, ViewModel), makeFilter(filter1, columns, ViewModel), makeFilter(filter2, columns, ViewModel));
            if (filter1 != "")
                return string.Format("({0} OR {1})", makeFilter(filter1, columns, ViewModel), makeFilter(filterExpression, columns, ViewModel));
            if (filter2 != "")
                return string.Format("({0} OR {1})", makeFilter(filterExpression, columns, ViewModel), makeFilter(filter2, columns, ViewModel));
            return "1=1";
        }

        private static string makeFilter(string filterExpression, string columns, object ViewModel)
        {
            string newFilter = "";
            if (string.IsNullOrEmpty(columns)) return newFilter;

            double filterDouble;
            bool isNumeric = double.TryParse(filterExpression, out filterDouble);

            string[] columnArr = columns.Split(',');
            if (columnArr.Length == 0) return "1=1";

            string[] filterArr = filterExpression.Replace("','",",").Split(',');
            foreach (string col in columnArr)
            {
                string filedName = col;
                if(col.Contains("."))
                {
                    string[] col_arr = col.Split('.');
                    filedName = col_arr[col_arr.Length - 1];
                }
                PropertyInfo pi = ViewModel.GetType().GetProperty(filedName);
                if (pi == null) throw new Exception(string.Format("Property with name '{0}' not found in ViewModel", filedName));
                if (pi.PropertyType == typeof(string))
                {
                    newFilter += " ( ";
                    foreach (string filter in filterArr)
                    {
                        newFilter += string.Format(" {0} LIKE N'%{1}%'&", col, filter);
                    }
                    newFilter = newFilter.TrimEnd('&').Replace("&"," AND ") + " )|";
                }
                else
                {
                    if (isNumeric)
                    {
                        newFilter += string.Format(" {0}={1}|", col, filterExpression);
                    }
                }

            }
            newFilter = " ( " + newFilter.TrimEnd('|').Replace("|", " OR ") + " ) ";
            return newFilter;
        }

        public int GetCount<T>(string sql, string filterExpression, string columns)
        {
            T ViewModel = Reflect.GetInstance<T>();
            filterExpression = getFilter(filterExpression, columns, ViewModel);
            string baked_sql = "SELECT COUNT(*)  FROM ( {sqlExpression} ) as DerivedName WHERE {filterExpression} ";
            baked_sql = Base.Extension.StringExtension.StringExtension.NamedStringFormat(baked_sql, new { sqlExpression = sql, filterExpression = filterExpression });
            DBHelper.SQLServer.DirectDB db = new DBHelper.SQLServer.DirectDB(_connectionString);
            int count = int.Parse(db.ExecuteScalar(CommandType.Text, baked_sql));
            db.Close();
            return count;
        }

        private string ApplyYeKeReverse(string input)
        {
            if (input.Contains("ي") || input.Contains("ك"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ي", "ی").Replace("ك", "ک");
            }
            if (input.Contains("ی") || input.Contains("ک"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ی", "ي").Replace("ک", "ك");
            }
            return "";

        }

        private string ApplyYeKe(string input)
        {
            if (input.Contains("ي") || input.Contains("ك"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ي", "ی").Replace("ك", "ک");
            }
            return "";

        }
    }
}