using System;
using System.Collections.Generic;
using System.Text;

namespace ManticFramework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ManticSqlTable : Attribute
    {
        public string Table { get; set; }
    }
}
