using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBHelper.Attributes
{
    /// <summary>
    /// Summary description for FieldMetaDataAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DBFieldMetadataAttribute : Attribute
    {
        public string _Label;
        public string Label
        {
            get { return _Label; }
            set{_Label = value;}
        }
        private bool _hide = false;
        private bool _nonTableField = false;
        private int _ordering = 0;
        public bool Hide { get { return _hide; } set { _hide = value; } }
        public bool NonTableField { get { return _nonTableField; } set { _nonTableField = value; } }
        public int Ordering { get { return _ordering; } set { _ordering = value; } }
        public DBFieldMetadataAttribute()
        {
        }
    }
}