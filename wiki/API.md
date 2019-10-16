# Constructors
| Name | Description |
| --- | --- |
| `Mantic(string)` | Takes a connection string|

# Properties
_No Public Properties_

# Methods
| Name | Description |
| --- | --- |
| `All<T>()` | Returns all elements in a table as type `T`. `T` must have been registered using `ManticSqlTable` and have an empty constructor. |
| `ConvertTo<T>(object)*` | Private helper function that converts an object to a class of type `T`. Useful for mapping `DBNull` to `null` and handling `Datetime`. |
| `ExecuteNonQueryStoredProcedure(string, Dictionary<string, object> = null)*` | Accepts a string of a stored procedure registered for non-query use. Parameters are passed via the dictionary where the key is the parameter type and the value is the parameter. Parameters will be converted to types specified when procedure was registered. Any parameters not supplied will be set to null. If `Dictionary` is not provided, all parameters will be set to null. |
| `ExecuteScalarStoredProcedure<T>(string, Dictionary<string, object>)*` | Executes a scalar stored procedure where the name of the procedure is passed as a `string`, that has been registered for scalar use. Returns an object of type `T`. `T` should be nullable. Parameters are passed via the dictionary where the key is the parameter type and the value is the parameter. Parameters will be converted to types specified when procedure was registered. Any parameters not supplied will be set to null. If `Dictionary` is not provided, all parameters will be set to null. |
| `ExecuteStoredProcedure<T>(string, Dictionary<string, object>)*` | Executes a query stored procedure where the name of the procedure is passed as a `string` and returns the results as a list of type `T`. `T` must have an empty constructor. Parameters are passed via the dictionary where the key is the parameter type and the value is the parameter. Parameters will be converted to types specified when procedure was registered. Any parameters not supplied will be set to null. If `Dictionary` is not provided, all parameters will be set to null. |
| `Fill<T>(DataTable)` | Takes a `DataTable` and converts it to an `IEnumerable` that contains a list of objects of type `T`. `T` must have an empty constructor. |
| `Fill<T>(SqlDataReader)*` | Takes a `SqlDataReader`, asynchronously reads from it, and converts the stream to an `IEnumerable` that contains a list of objects of type `T`. `T` must have an empty constructor. |
| `FillSqlCommand(SqlCommand, ManticStoredProcedure, Dictionary<string, object>)` | Private method that takes a `SqlCommand` and fills its parameters as specified upon registration. |
| `HasMappedTable<T>()` | Returns a boolean that signifies if a class was registered with the `ManticSqlTable` attribute |
| `Insert<T>(T data)*` | Insert the `data` object into the table associated with type `T`. `ManticSqlTable` must have been used when this class was registered. The `IgnoreOnInsert` value can be set on `ManticSqlColumn` to avoid having that property be inserted. |
| `IsRegistered<T>(Type)` | Returns a boolean that signifies if the class has been registered or not |
| `IsStoredProcedureRegistered(string)` | Returns a boolean that specifies if the string passed to it is the name of a registered stored procedure. |
| `NonQuery(SqlCommand)*` | Takes a `SqlCommand` and executes it as a non query. |
| `Query<T>(SqlCommand)*` | Takes a filled `SqlCommand` and returns the results of that command as a list of type `T`. `T` must have an empty constructor. Query is executed using a `SqlDataAdapter` as a bulk fetch. |
| `Query<T>(string, bool)*` | Takes a `string` representing a plaintext query and an optional boolean specifying whether it is a stored procedure or not (default is false). Returns a list of type `T`. Query is executed using a `SqlDataAdapter` as a bulk fetch. |
| `QueryStream<T>(SqlCommand)*` | Takes a filled `SqlCommand` and returns the result of that command as a list of type `T`. `T` must have an empty constructor. Query is executed using `SqlDataReader` as a stream. |
| `QueryStream<T>(string, bool)*` | Takes a `string` as a plaintext query and a bool specifying if that query is a stored procedure or not. Returns a list of type `T`. `T` must have an empty constructor. Query is executed using a `SqlDataReader` as a stream. |
| `Register<T>()` | Register a class. This must be called before that class is used within Mantic |
| `RegisterStoredProcedure<T>(string, Dictionary<string,(DbType, int?)>, bool = false, bool = false)` | Register a stored procedure that returns results of type `T`. Pass parameters in the dictionary where the key is the parameter name and the value is a `Tuple` consisting of the parameter type and length. For types without lengths, set length to `null`. For types with `MAX` lengths, set length to `-1`. For non-queries, set `isNonQuery = true` and for scalar queries, set `isScalar = true` and pass `int?` as `T`. `T` must have an empty constructor. |
| `Scalar<T>(SqlCommand)*` | Private helper function that executes a scalar stored procedure and returns an object of type `T` where `T` should be nullable. |

*\*async - must be `await`ed or be called with `.Result` appended to ensure execution*

# `DbType`
Maps values between `SqlDbType` and Mantic compatible values

# Data Attributes
## `ManticSqlColumn`
Cannot be inherited or placed more than once on a C# property

| Property | Description |
| --- | --- |
| `ColumnLength` | The length of the column in the SQL Server. |
| `IgnoreOnInsert` | Do not insert this column when `Insert<T>` is called. Useful for columns that represent internal IDs. |
| `Name` | The name of the column the property should map to. |
| `TryGetDbType(out SqlDbType?)` | Internal method to convert the `DbType` to `SqlDbType`. |
| `Type` | The `DbType` of the column in Sql Server. |

## `ManticSqlTable`
Cannot be inherited or placed more than once on a C# property. This attribute enables `All<T>` and `Insert<T>` to be used.

| Property | Description |
| --- | --- | 
| `Table` | The table that this class maps to in SQL Server. | 