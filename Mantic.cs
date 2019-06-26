using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global

namespace ManticFramework
{
    public class Mantic
    {
        //Mappings and caches
        private readonly Dictionary<string, Dictionary<string, Mapping>> _columnMappings;
        private readonly Dictionary<string, string> _tableMappings;
        private readonly Dictionary<string, string> _selectQueries;
        private readonly Dictionary<string, string> _insertQueries;
        private readonly Dictionary<string, ManticStoredProcedure> _storedProcedures;

        private string ConnectionString { get; }
        
        public Mantic(string connectionString = null)
        {
            _columnMappings = new Dictionary<string, Dictionary<string, Mapping>>();
            _tableMappings = new Dictionary<string, string>();
            _selectQueries = new Dictionary<string, string>();
            _insertQueries = new Dictionary<string, string>();
            _storedProcedures = new Dictionary<string, ManticStoredProcedure>();
            ConnectionString = connectionString;
        }

        //Check if a class has been Registered
        private bool IsRegistered(Type t) => _columnMappings.ContainsKey(t.FullName);

        //Check if a class has been Registered using ManticSqlTable
        public bool HasMappedTable<T>() => _tableMappings.ContainsKey(typeof(T).FullName);
        
        /*
         * Register a class by extracting type information which includes class name, property names,
         * and Mantic data annotation values
         */
        public void Register<T>()
        {
            var type = typeof(T);
            var props = type.GetProperties();
            var map = new Dictionary<string, Mapping>();
            
            foreach (var prop in props)
            {
                var l = prop.GetCustomAttributes(typeof(ManticSqlColumn), true);

                //Make sure any internal properties or ones that do not have the attribute do not get mapped
                if (l.Length != 1 || !(l[0] is ManticSqlColumn attrib))
                {
                    continue;
                }

                var propType = prop.PropertyType;

                //if adding support for non attributed props, add optional bool param addAll and skip nullable check
                if (propType.IsValueType && Nullable.GetUnderlyingType(propType) == null)
                {
                    throw new ArgumentException("Properties having the SqlColumnAttribute must be nullable!");
                }

//                var propName = $"{type.FullName}.{prop.Name}";

                attrib.TryGetDbType(out var s);

                map.Add(prop.Name, new Mapping
                {
//                    PropertyType = propType,
                    SqlColumnName = attrib.Name,
                    SqlColumnType = s,
                    SqlColumnLength = attrib.ColumnLength,
                    IgnoreOnInsert = attrib.IgnoreOnInsert
                });
            }

            _columnMappings.Add(type.FullName, map);

            //If class has ManticSqlTable, build and cache relevant queries
            var x = type.GetCustomAttributes(typeof(ManticSqlTable), true);
            if (x.Length > 1)
            {
                throw new ArgumentException("A single class cannot contain multiple SqlTable attributes! Check inheritance.");
            }
            else if (x.Length == 1 && (x[0] is ManticSqlTable t))
            {
                _tableMappings.Add(type.FullName, t.Table);
                _selectQueries.Add(type.FullName, $"SELECT * FROM {_tableMappings[typeof(T).FullName]}");


                var builder = new StringBuilder();
                builder.Append("INSERT INTO ")
                    .Append(_tableMappings[type.FullName])
                    .Append('(')
                    .Append(string.Join(',', _columnMappings[type.FullName]
                        .Where(i => !(i.Value.IgnoreOnInsert.HasValue && i.Value.IgnoreOnInsert.Value))
                        .Select(i => $"[{i.Value.SqlColumnName}]")))
                    .Append(") VALUES(")
                    .Append(string.Join(',', _columnMappings[type.FullName]
                        .Where(i => !(i.Value.IgnoreOnInsert.HasValue && i.Value.IgnoreOnInsert.Value))
                        .Select(i => $"@{i.Value.SqlColumnName}")))
                    .Append(')');
                _insertQueries.Add(type.FullName, builder.ToString());
            }

        }

        /*
         * Take a datatable and convert it's rows to a list of objects
         */
        public IEnumerable<T> Fill<T>(DataTable dt) where T : new()
        {
            var type = typeof(T);

            if (!IsRegistered(type))
            {
                throw new ArgumentException("Class not registered!");
            }

            foreach (DataRow row in dt.Rows)
            {
                var obj = new T();
//                var props = obj.GetType().GetProperties();
                foreach (var (key, value) in _columnMappings[type.FullName])
                {
                    if (dt.Columns.Contains(value.SqlColumnName))
                    {
                        type.GetProperty(key).SetValue(obj, row[value.SqlColumnName] != DBNull.Value ? row[value.SqlColumnName] : null);
                    }
                    else
                    {
                        type.GetProperty(key).SetValue(obj, null);
                    }
                }

                yield return obj;
            }

        }

        public async Task<IEnumerable<T>> Fill<T>(SqlDataReader reader) where T : new()
        {
            var results = new List<T>();
            var type = typeof(T);
            if (!IsRegistered(type))
            {
                throw new ArgumentException($"Class {type.FullName} not registered!");
            }

            var columns = reader.GetColumnSchema().Select(x => x.ColumnName).ToDictionary(x=>x, null);

            while (await reader.ReadAsync())
            {
                var obj = new T();
//                var props = obj.GetType().GetProperties();
                foreach (var (key, value) in _columnMappings[type.FullName])
                {
                    var val = reader[value.SqlColumnName];
                    if (columns.ContainsKey(value.SqlColumnName) && val!=DBNull.Value)
                    {
                        type.GetProperty(key).SetValue(obj, reader[value.SqlColumnName]);
                    }
                    else
                    {
                        type.GetProperty(key).SetValue(obj, null);
                    }
                }

                results.Add(obj);

            }

            return results;
        }

        //Execute a sql command that doesn't return anything
        public async Task NonQuery(SqlCommand command) {
            if (ConnectionString == null)
            {
                throw new Exception("Connection string not set!");
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "You need a non-null sql command to query a database.");
            }

            //var dt = new DataTable();

            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                command.Connection = conn;
                await command.ExecuteNonQueryAsync();
            }

//            return null;
        }

        /*
         * Execute a sql string that returns a table
         */
        public async Task<IEnumerable<T>> Query<T>(string command, bool storedProcedure = false) where T : new()
        {
            using (var cmd = new SqlCommand(command))
            {

                if (storedProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                return await Query<T>(cmd);
            }
        }

        /*
         * Execute a sql command that returns a table
         */
        public async Task<IEnumerable<T>> Query<T>(SqlCommand command) where T : new()
        {
            if (ConnectionString == null)
            {
                throw new Exception("Connection string not set!");
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "You need a non-null sql command to query a database.");
            }

            var dt = new DataTable();

            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                command.Connection = conn;
                new SqlDataAdapter(command).Fill(dt);
            }

            return Fill<T>(dt);
        }

        public async Task<IEnumerable<T>> QueryStream<T>(string command, bool storedProcedure = false) where T : new()
        {
            using (var cmd = new SqlCommand(command))
            {

                if (storedProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                return await QueryStream<T>(cmd);
            }
        }

        public async Task<IEnumerable<T>> QueryStream<T>(SqlCommand cmd) where T : new()
        {
            if (ConnectionString == null)
            {
                throw new Exception("Connection string not set!");
            }

            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd), "You need a non-null sql command to query a database.");
            }

            using (var conn = new SqlConnection(ConnectionString))
            {
                cmd.Connection = conn;
                await conn.OpenAsync();
                return await Fill<T>(cmd.ExecuteReader());
            }
        }

        public async Task<IEnumerable<T>> All<T>() where T : new()
        {
            if (!IsRegistered(typeof(T)))
            {
                throw new ArgumentException("Class is not registered!");
            }

            if (!HasMappedTable<T>())
            {
                throw new ArgumentException("Class does not have a mapped table. Use Sql Table.");
            }

            return await Query<T>(_selectQueries[typeof(T).FullName]);
        }

        /*
         * Insert an object into the database
         */
        public async Task Insert<T>(T data) where T : new()
        {
            var t = data.GetType();

            if (!HasMappedTable<T>())
            {
                throw new ArgumentException("Class has no mapped table. Use SqlTable.");
            }
            if (!IsRegistered(t))
            {
                throw new ArgumentException("Class is not registered");
            }

            using (var cmd = new SqlCommand(_insertQueries[t.FullName]))
            {

                foreach (var (key, value) in _columnMappings[t.FullName])
                {
//                    var obj = new object();
                    var val = t.GetProperty(key).GetValue(data, null);

                    if (!value.SqlColumnType.HasValue)
                    {
                        throw new ArgumentException("To use insert you must specify a sql column data type! :'(");
                    }

                    if (val == null)
                    {
                        cmd.Parameters.Add($"@{value.SqlColumnName}", value.SqlColumnType.Value).Value = DBNull.Value;
                    }
                    else if (value.SqlColumnLength == null)
                    {
                        cmd.Parameters.Add($"@{value.SqlColumnName}", value.SqlColumnType.Value).Value = val;
                    }
                    else
                    {
                        cmd.Parameters.Add($"@{value.SqlColumnName}", value.SqlColumnType.Value, value.SqlColumnLength.Value).Value = val;
                    }
                }

                using (var conn = new SqlConnection(ConnectionString))
                {
                    cmd.Connection = conn;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /*
         * Map stored procedure parameters and class type
         */
        public void RegisterStoredProcedure<T>(string name, Dictionary<string, (DbType, int?)> procedure, bool isNonQuery=false, bool isScalar = false) where T:new() {
            if (IsStoredProcedureRegistered(name)) {
                throw new ArgumentException("Stored Procedure already registered!");
            }

            var t = typeof(T);
            if (!IsRegistered(t)) {
                Register<T>();
            }

            _storedProcedures.Add(name,
                new ManticStoredProcedure {
                    Mappings = procedure,
//                    ReturnType = t.FullName,
                    IsNonQuery = isNonQuery,
                    IsScalar = isScalar
                });
        }

        /*
         * Front facing interface for executing a stored procedure that does not return anything
         */
        public async Task ExecuteNonQueryStoredProcedure(string name, Dictionary<string, object> parameters = null) {
            if (!IsStoredProcedureRegistered(name)) {
                throw new ArgumentException("Stored Procedure Not Registered");
            }

            using (var cmd = new SqlCommand(name))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                //var cmd = new SqlCommand(name)
                //{
                //    CommandType = CommandType.StoredProcedure
                //};

                var proc = _storedProcedures[name];

                if (!proc.IsNonQuery)
                {
                    throw new ArgumentException("Stored Procedure Marked as Having Results");
                }

                FillSqlCommand(cmd, proc, parameters);

                await NonQuery(cmd);
            }
        }

        /*
         * Create a SqlCommand using stored procedure mappings and the parameters passed externally
         */
        private static void FillSqlCommand(SqlCommand cmd, ManticStoredProcedure proc, Dictionary<string, object> parameters)
        {
            parameters = parameters ?? new Dictionary<string, object>();
            foreach (var (key, (dbType, length)) in proc.Mappings)
            {
                Util.TryGetDbType(dbType, out var t);
                if (t == null)
                {
                    throw new Exception("Cannot create Sql Command. Database type conversion failed.");
                }

                if (parameters.ContainsKey(key))
                {
                    if (length.HasValue)
                    {
                        cmd.Parameters.Add(key, t.Value, length.Value).Value = parameters[key]??DBNull.Value ;
                    }
                    else
                    {
                        cmd.Parameters.Add(key, t.Value).Value = parameters[key] ?? DBNull.Value;
                    }
                }
                else
                {
                    cmd.Parameters.Add(key, t.Value).Value = DBNull.Value;
                }
            }

//            foreach (var entry in parameters)
//            {
//                var mapping = proc.Mappings[entry.Key];
//                SqlDbType? t;
//                Util.TryGetDbType(mapping.Item1, out t);
//                if (mapping.Item2.HasValue)
//                {
//                    cmd.Parameters.Add(entry.Key, t.Value, mapping.Item2.Value).Value = entry.Value;
//                }
//                else
//                {
//                    cmd.Parameters.Add(entry.Key, t.Value).Value = entry.Value;
//                }
//            }
        }

        /*
         * Execute a stored procedure that returns one value
         */
        public async Task<T> ExecuteScalarStoredProcedure<T>(string name, Dictionary<string, object> parameters = null){
            if (!IsStoredProcedureRegistered(name)) {
                throw new ArgumentException("Stored Procedure Not Registered");
            }

            var proc = _storedProcedures[name];

            if (!proc.IsScalar) {
                throw new ArgumentException("Stored Procedure Not Registered for Scalar Result");
            }

            using (var cmd = new SqlCommand(name)) {
                cmd.CommandType = CommandType.StoredProcedure;

                FillSqlCommand(cmd, proc, parameters);

                return await Scalar<T>(cmd);
            }

        }

        /*
         * Execute stored procedure that returns a table
         */
        public async Task<IEnumerable<T>> ExecuteStoredProcedure<T>(string name, Dictionary<string, object> parameters = null) where T : new() {
            if (!IsStoredProcedureRegistered(name)) {
                throw new ArgumentException("Stored Procedure Not Registered!");
            }

            if (!IsRegistered(typeof(T))) {
                throw new ArgumentException("Class is Not Registered!");
            }

            using (var cmd = new SqlCommand(name)) {
                cmd.CommandType = CommandType.StoredProcedure;

                var proc = _storedProcedures[name];


                FillSqlCommand(cmd, proc, parameters);

                return await Query<T>(cmd);
            }
        }

        /*
         * Check if a stored procedure has been registered
         */
        public bool IsStoredProcedureRegistered(string name) {
            return _storedProcedures.ContainsKey(name);
        }

        /*
         * Internal method used to execute scalar Sql Commands
         */
        private async Task<T> Scalar<T>(SqlCommand cmd)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                cmd.Connection = conn;
                conn.Open();

                var obj = await cmd.ExecuteScalarAsync();
                return ConvertTo<T>(obj);
            }
        }

        /*
         * Internal method to help convert objects to the user defined types
         */
        private static T ConvertTo<T>(object obj)
        {
            if (obj == DBNull.Value)
            {
                return default(T);
            }

            var type = Nullable.GetUnderlyingType(typeof(T));

            if (type != null)
            {
                return (T)Convert.ChangeType(obj, type);
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }
    }

    internal class ManticStoredProcedure {
        public Dictionary<string, (DbType, int?)> Mappings { get; set; }

        public bool IsNonQuery { get; set; }

        public bool IsScalar { get; set; }

    }

    internal class Mapping
    {
        public string SqlColumnName { get; set; }

        public SqlDbType? SqlColumnType { get; set; }

        public int? SqlColumnLength { get; set; }

        public bool? IgnoreOnInsert { get; set; }
    }
}
