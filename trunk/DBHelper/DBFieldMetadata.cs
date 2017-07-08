using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBHelper.Attributes;

namespace DBHelper
{
    public class DBFieldMetadata
    {
        public string Key { get; set; }
        public DBFieldMetadataAttribute Attrib { get; set; }
    }
}