using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace ManticFramework
{

    //https://github.com/dotnet/csharplang/blob/master/spec/attributes.md#attribute-parameter-types
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ManticSqlColumn : Attribute
    {
        public string Name { get; set; }

        //public SqlDbType? Type { get; set; }

        public int ColumnLength { get; set; }

        public bool IgnoreOnInsert { get; set; }

        public DbType Type { get; set; }

        internal bool TryGetDbType(out SqlDbType? x) {
            if (!Enum.TryParse(typeof(SqlDbType), Type.ToString(), out object obj))
            {
                x = null;
                return false;
            }

            x = (SqlDbType)obj;
            return true;

        }
    }

    internal static class Util
    {
        internal static bool TryGetDbType(DbType type, out SqlDbType? x)
        {
            if (!Enum.TryParse(typeof(SqlDbType), type.ToString(), out object obj))
            {
                x = null;
                return false;
            }

            x = (SqlDbType)obj;
            return true;
        }
    }

    public enum DbType {
        None, BigInt, Bit, Char, Date, DateTime, DateTime2, DateTimeOffset, Decimal, Float, Image, Int, Money, NChar, NText, NVarChar, Real, SmallDateTime, SmallInt, SmallMoney, Structured, Text, Time, Timestamp, TinyInt, Udt, UniqueIdentifier, VarBinary, VarChar, Variant, Xml
    }
}
