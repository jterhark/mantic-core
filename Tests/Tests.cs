using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ManticFramework.Tests.Models;

namespace ManticFramework.Tests
{
    public class Tests
    {
        private Mantic _mantic;

        private const string ConnectionString =
            "Data Source=sa-vm-appdev;Initial Catalog=divvy;Integrated Security=True";

        [SetUp]
        public void Setup()
        {
            _mantic = new Mantic(ConnectionString);
            _mantic.Register<Station>();
            _mantic.Register<Trip>();
            _mantic.RegisterStoredProcedure<int?>("NonQueryTest", new Dictionary<string, (DbType, int?)>()
            {
                ["@data"] = (DbType.NVarChar, 255)
            }, isNonQuery: true);
            _mantic.RegisterStoredProcedure<int?>("GetStationNameFromId", new Dictionary<string, (DbType, int?)>()
            {
                ["@station_id"] = (DbType.Int, null)
            }, isScalar: true);
            _mantic.RegisterStoredProcedure<Trip>("GetTripsFromStation", new Dictionary<string, (DbType, int?)>()
            {
                ["@from_station"] = (DbType.NVarChar, 50)
            });
        }

        [Test]
        public async Task NonQuery()
        {
            await _mantic.NonQuery(new SqlCommand("SELECT 1"));
        }

        [Test]
        public async Task NonQueryStoredProc()
        {
            await _mantic.ExecuteNonQueryStoredProcedure("NonQueryTest", new Dictionary<string, object>()
            {
                ["@data"] = "Test"
            });
            await _mantic.ExecuteNonQueryStoredProcedure("NonQueryTest", new Dictionary<string, object>()
            {
                ["@data"] = null
            });
        }

        [Test]
        public async Task ScalarStoredProc()
        {
            var station = await _mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId",
                new Dictionary<string, object>()
                {
                    ["@station_id"] = 15
                });
            Assert.AreEqual("Racine Ave & 18th St", station);

            station = await _mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId");
            Assert.Null(station);

            station = await _mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId",
                new Dictionary<string, object>());
            Assert.Null(station);
        }

        [Test]
        public async Task QueryStoredProc()
        {
            var trips = await _mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation",
                new Dictionary<string, object>()
                {
                    ["@from_station"] = "Buckingham Fountain"
                });
            Assert.AreEqual(683, trips.Count());

            trips = await _mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation");
            Assert.AreEqual(0, trips.Count());

            trips = await _mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation", new Dictionary<string, object>());
            Assert.AreEqual(0, trips.Count());
        }

        [Test]
        public async Task SelectStations()
        {
            var stations = await _mantic.All<Station>();
            Assert.AreEqual(608, stations.Count());
        }

        [Test]
        public async Task SelectTrips()
        {
            var trips = await _mantic.All<Trip>();
            Assert.AreEqual(642686, trips.Count());
        }

        [Test]
        public async Task QueryStreamSelectTrips()
        {
            var trips = await _mantic.QueryStream<Trip>("SELECT * FROM Trips");
            Assert.AreEqual(642686, trips.Count());
        }
    }
}