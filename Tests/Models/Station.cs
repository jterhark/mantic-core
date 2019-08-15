using ManticFramework;

namespace ManticFramework.Tests.Models
{
    [ManticSqlTable(Table = "Stations")]
    public class Station
    {
        [ManticSqlColumn(Name = "ID", Type=DbType.Int, IgnoreOnInsert = true)]
        public int? Id { get; set; }

        [ManticSqlColumn(Name="Station_Name", Type = DbType.NVarChar,
            ColumnLength = 50)]
        public string Name { get; set; }
        
        [ManticSqlColumn(Name = "Address", Type = DbType.NVarChar, ColumnLength = 50)]
        public string Address { get; set; }
        
        [ManticSqlColumn(Name = "Total_Docks", Type = DbType.Int)]
        public int? TotalDocks { get; set; }

        [ManticSqlColumn(Name = "Docks_in_Service", Type = DbType.Int)]
        public int? WorkingDocks { get; set; }

        [ManticSqlColumn(Name = "Status", Type = DbType.NVarChar, ColumnLength = 50)]
        public string Status { get; set; }

        [ManticSqlColumn(Name = "Latitude", Type = DbType.Float)]
        public double? Latitude { get; set; }

        [ManticSqlColumn(Name = "Longitude", Type = DbType.Float)]
        public double? Longitude { get; set; }

        [ManticSqlColumn(Name = "Zip_Codes", Type = DbType.Int)]
        public int? ZipCode { get; set; }

        [ManticSqlColumn(Name = "Wards", Type = DbType.Int)]
        public int? Ward { get; set; }
    }
}