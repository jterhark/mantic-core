# Setting Up Your Data Model
## Prerequisites
- Familiarity with C\#
- Project is in .NET Core
- Database is in SQL Server, has been created, and its structure defined
- Valid connection string
- `S:\SATechnology\Projects\Development\Nuget Packages` has been added as a Nuget package source and the `ManticFramework` nuget package has been installed in your project.

## Example Model
```c#
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
    
    public string SomethingElse { get; set; }
}
```

## Explanation
`ManticSqlColumn` maps each property to a column in the database where `Name` is the column name, `Type` is the column type and `ColumnLength` is the column length.
If you are using a type that does not require a length, the `ColumnLength` can be ignored. On the other hand, if you need `MAX` length, set `ColumnLength` to -1.
All the properties that are annotated with Mantic annotations must be nullable (`?`). This means that `null` can be assigned to any of the above properties.
Any properties not mapped will be ignored and set to `null` upon instantiation. Any columns mapped, but not present in query results will also be set to `null`.
Notice that the Id field has the property `IgnoreOnInsert` set to `true`. This means that when a new value is inserted into the DB, Mantic will not insert an id.
This is particularly valuable when the id column is marked as an identity column in the db.

`ManticSqlTable` maps a class to a table in the database and is optional, but including this will allow the use of `Insert<T>` and `All<T>`.
