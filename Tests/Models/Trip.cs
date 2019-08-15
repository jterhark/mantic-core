using System;
using ManticFramework;
using MCol = ManticFramework.ManticSqlColumn;
using MTable = ManticFramework.ManticSqlTable;

namespace ManticFramework.Tests.Models
{
    [MTable(Table="Trips")]
    public class Trip
    {
        [MCol(Name = "trip_id", Type = DbType.Int)]
        public int? TripId { get; set; }
        
        [MCol(Name = "start_time", Type = DbType.DateTime2, ColumnLength = 7)]
        public DateTime? Start { get; set; }

        [MCol(Name = "end_time", Type = DbType.DateTime2, ColumnLength = 7)]
        public DateTime? End { get; set; }

        [MCol(Name = "bikeid", Type = DbType.Int)]
        public int? BikeId { get; set; }

        [MCol(Name = "tripduration", Type = DbType.NVarChar, ColumnLength = 50)]
        public string Duration { get; set; }

        [MCol(Name = "from_station_id", Type = DbType.Int)]
        public int? FromStationId { get; set; }

        [MCol(Name = "to_station_id", Type = DbType.Int)]
        public int? ToStationId { get; set; }

        [MCol(Name = "usertype", Type = DbType.NVarChar, ColumnLength = 50)]
        public string UserType { get; set; }

        [MCol(Name = "gender", Type = DbType.NVarChar, ColumnLength = 50)]
        public string Gender { get; set; }

        [MCol(Name = "birthyear", Type = DbType.Int)]
        public int? BirthYear { get; set; }
    }
}