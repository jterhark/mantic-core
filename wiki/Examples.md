# Setup

1. [Create your data model](Setup-Data-Model)
2. Instantiate Mantic.  
    ```c#
    var mantic = new Mantic(ConnectionString);
    ``` 
3. Register classes. Keep Mantic instantiated for the life of the application to run this costly step only once.
    ```c#
    mantic.Register<Station>();
    mantic.Register<Trip>();
    ```  
4. Register stored procedures. Again this is costly so only do this once.
    ```c#
    //standard query
    mantic.RegisterStoredProcedure<Trip>("GetTripsFromStation", new Dictionary<string, (DbType, int?)>()
    {
        ["@from_station"] = (DbType.NVarChar, 50)
    });
    
    //scalar
    mantic.RegisterStoredProcedure<int?>("GetStationNameFromId", new Dictionary<string, (DbType, int?)>()
    {
        ["@station_id"] = (DbType.Int, null)
    }, isScalar: true);
 
    //nonquery
    _mantic.RegisterStoredProcedure<int?>("NonQueryTest", new Dictionary<string, (DbType, int?)>()
    {
        ["@data"] = (DbType.NVarChar, 255)
    }, isNonQuery: true);
    ```
# Select All From a table
##### Asynchronous
```c#
var all = mantic.All<Station>().Result;
```
##### Synchronous
```c#
var all = await mantic.All<Station>();
```

# Insert
##### Asynchronous
```c#
await mantic.Insert(new Station(){Name="Test Station"});
```
##### Synchronous
```c#
mantic.Insert(new Station(){Name="Test Station"}).Wait();
```

# Stored Procedures
### Query Stored Procedure
##### Asynchronous
```c#
//with parameters
var trips = await mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation",
    new Dictionary<string, object>()
        {
            ["@from_station"] = "Buckingham Fountain"
        });

//without parameters
trips = await _mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation");
```
##### Synchronous
```c#
//with parameters
var trips = mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation",
    new Dictionary<string, object>()
        {
            ["@from_station"] = "Buckingham Fountain"
        }
    ).Result;

//without parameters
trips = _mantic.ExecuteStoredProcedure<Trip>("GetTripsFromStation").Result;
```

### Scalar Stored Procedure
##### Asynchronous
```c#
//with parameters
var station = await mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId",
    new Dictionary<string, object>()
    {
        ["@station_id"] = 15
    });

//without parameters
station = await mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId");
```
##### Synchronous
```c#
//with parameters
var station = mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId",
    new Dictionary<string, object>()
    {
        ["@station_id"] = 15
    }).Result;

//without parameters
station = mantic.ExecuteScalarStoredProcedure<string>("GetStationNameFromId").Result;
```

### Non-Query Stored Procedure
##### Asynchronous
```c#
//with parameters
await mantic.ExecuteNonQueryStoredProcedure("NonQueryTest", new Dictionary<string, object>()
{
    ["@data"] = null
});

//without parameters
await mantic.ExecuteNonQueryStoredProcedure("NonQueryTest");
```
##### Synchronous
```c#
//with parameters
mantic.ExecuteNonQueryStoredProcedure("NonQueryTest", new Dictionary<string, object>()
{
    ["@data"] = null
}).Wait();

//without parameters
mantic.ExecuteNonQueryStoredProcedure("NonQueryTest").Wait();
```

# Ad Hoc `SqlCommand` Query
```c#
//SqlDataAdapter
var stations = await mantic.Query<Station>(new SqlCommand(...));

//SqlDataReader
var stations = await mantic.QueryStream<Station>(new SqlCommand(...));
```

# Plain Text Query
```c#
//SqlDataAdapter
var trips = await mantic.Query<Trip>("SELECT * FROM Trips");
var trips = await mantic.Query<Trip>("UpdateRecord", storedProcedure: true);

//SqlDataReader
var trips = await mantic.QueryStream<Trip>("SELECT * FROM Trips");
var trips = await mantic.QueryStream<Trip>("UpdateRecord", storedProcedure: true);
```