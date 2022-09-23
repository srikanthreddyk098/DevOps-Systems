using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.CSharp.RuntimeBinder;

namespace CalpineAzureDashboard.Data
{
//Modified from https://github.com/ericdc1/Dapper.SimpleCRUD

    public class DbConnection : IDbConnection
    {
        #region Private fields

        private readonly System.Data.IDbConnection _conn;
        private Dialect _dialect = Dialect.SqlServer;
        private IDbTransaction _transaction;

        private string _encapsulation;
        private string _getIdentitySql;
        private string _getPagedListSql;

        private readonly ConcurrentDictionary<Type, string> _tableNames = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<string, string> _columnNames = new ConcurrentDictionary<string, string>();

        private ITableNameResolver _tableNameResolver;
        private IColumnNameResolver _columnNameResolver;

        private static int DefaultCommandTimeout =>
            int.TryParse(ConfigurationManager.AppSettings["commandTimeoutInSeconds"], out var timeout) ? timeout : 60;

        #endregion

        #region Constructor

        public DbConnection(System.Data.IDbConnection conn, Dialect dialect = Dialect.SqlServer)
        {
            _conn = conn;
            SetDialect(dialect);

            _tableNameResolver = new TableNameResolver(_encapsulation);
            _columnNameResolver = new ColumnNameResolver(_encapsulation);
        }

        /// <summary>
        /// Returns the current dialect name
        /// </summary>
        /// <returns></returns>
        public string GetDialect()
        {
            return _dialect.ToString();
        }

        /// <summary>
        /// Configures the sql flavor to use.
        /// </summary>
        /// <param name="dialect"></param>
        public void SetDialect(Dialect dialect)
        {
            switch (dialect) {
                case Dialect.PostgreSql:
                    _dialect = Dialect.PostgreSql;
                    _encapsulation = "\"{0}\"";
                    _getIdentitySql = "SELECT LASTVAL() AS id";
                    _getPagedListSql =
                        "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                    break;
                case Dialect.SqLite:
                    _dialect = Dialect.SqLite;
                    _encapsulation = "\"{0}\"";
                    _getIdentitySql = "SELECT LAST_INSERT_ROWID() AS id";
                    _getPagedListSql =
                        "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                    break;
                case Dialect.MySql:
                    _dialect = Dialect.MySql;
                    _encapsulation = "`{0}`";
                    _getIdentitySql = "SELECT LAST_INSERT_ID() AS id";
                    _getPagedListSql =
                        "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}";
                    break;
                case Dialect.SqlServer:
                    _dialect = Dialect.SqlServer;
                    _encapsulation = "[{0}]";
                    _getIdentitySql = "SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]";
                    _getPagedListSql =
                        "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
                    break;
                default:
                    throw new Exception("DbConnection must have a valid Sql dialect set.");
            }
        }

        #endregion

        #region Database Helpers

        /// <summary>
        /// Opens a transaction
        /// </summary>
        public virtual void BeginTransaction()
        {
            if (_conn.State == ConnectionState.Open) {
                if (_transaction == null) {
                    _transaction = _conn.BeginTransaction();
                }
            }
            else {
                _conn.Open();
                _transaction = _conn.BeginTransaction();
            }
        }

        /// <summary>
        /// Commits and closes the transaction
        /// </summary>
        public virtual void CommitTransaction()
        {
            try {
                _transaction?.Commit();
            }
            catch {
                _transaction?.Rollback();
                throw;
            }
            finally {
                _transaction?.Dispose();
                _conn.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _conn.Dispose();
            _transaction = null;
        }

        #endregion

        #region CRUD operations
        
        public virtual Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _conn.QueryFirstOrDefaultAsync<T>(query, parameters,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        public virtual Task<T> GetAsync<T>(string query, DbParameters parameters, int? commandTimeout = null)
        {
            return _conn.QueryFirstOrDefaultAsync<T>(query, parameters,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>By default filters on the Id column</para>
        /// <para>-Id column name can be overridden by adding an attribute on your primary key property [Key]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a single entity by a single id from table T</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a single entity by a single id from table T.</returns>
        public virtual Task<T> GetAsync<T>(object id, int? commandTimeout = null)
        {
            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Get<T> only supports an entity with a [Key] or Id property");

            var name = GetTableName(currenttype);
            var sb = new StringBuilder();
            sb.Append("Select ");
            //create a new empty instance of the type to get the base properties
            BuildSelect(sb, GetScaffoldableProperties<T>().ToArray());
            sb.AppendFormat(" from {0} where ", name);

            for (var i = 0; i < idProps.Count; i++) {
                if (i > 0)
                    sb.Append(" and ");
                sb.AppendFormat("{0} = @{1}", GetColumnName(idProps[i]), idProps[i].Name);
            }

            var parameters = new DynamicParameters();
            if (idProps.Count == 1)
                parameters.Add("@" + idProps.First().Name, id);
            else {
                foreach (var prop in idProps)
                    parameters.Add("@" + prop.Name, id.GetType().GetProperty(prop.Name)?.GetValue(id, null));
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"Get<{currenttype}>: {sb} with Id: {id}");

            return _conn.QueryFirstOrDefaultAsync<T>(sb.ToString(), parameters,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout, transaction: _transaction);
        }

        public virtual Task<IEnumerable<T>> GetCollectionAsync<T>(string query, DbParameters parameters,
            int? commandTimeout = null, CommandType commandType = CommandType.Text)
        {
            return _conn.QueryAsync<T>(query, parameters.DynamicParameters,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction, commandType: commandType);
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereObject"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns>Gets a list of entities with optional exact match where conditions</returns>
        public virtual Task<IEnumerable<T>> GetCollectionAsync<T>(object whereObject = null, int? commandTimeout = null,
            CommandType commandType = CommandType.Text)
        {
            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            var whereprops = GetAllProperties(whereObject).ToArray();
            sb.Append("Select ");
            //create a new empty instance of the type to get the base properties
            BuildSelect(sb, GetScaffoldableProperties<T>().ToArray());
            sb.AppendFormat(" from {0}", name);

            if (whereprops.Any()) {
                sb.Append(" where ");
                BuildWhere<T>(sb, whereprops, whereObject);
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"GetList<{currenttype}>: {sb}");

            return _conn.QueryAsync<T>(sb.ToString(), whereObject, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction, commandType: commandType);
        }

        public virtual Task<IEnumerable<T>> GetCollectionAsync<T>(string whereClause, int? commandTimeout = null, CommandType commandType = CommandType.Text)
        {
            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            sb.Append("Select ");
            //create a new empty instance of the type to get the base properties
            BuildSelect(sb, GetScaffoldableProperties<T>().ToArray());
            sb.AppendFormat(" from {0}", name);

            if (!string.IsNullOrEmpty(whereClause)) {
                sb.Append(" where ");
                sb.Append(whereClause);
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"GetList<{currenttype}>: {sb}");

            return _conn.QueryAsync<T>(sb.ToString(), commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction, commandType: commandType);
        }

        public virtual Task<IEnumerable<T>> GetCollectionAsync<T>(string query, object parameters = null, int? commandTimeout = null, CommandType commandType = CommandType.Text)
        {
            return _conn.QueryAsync<T>(query, parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction, commandType: commandType);
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>orderby is a column or list of columns to order by ex: "lastname, age desc" - not required - default is by primary key</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns>Gets a paged list of entities with optional exact match where conditions</returns>
        public virtual Task<IEnumerable<T>> GetPagedCollectionAsync<T>(int pageNumber, int rowsPerPage,
            string conditions,
            string orderby, object parameters = null, int? commandTimeout = null,
            CommandType commandType = CommandType.Text)
        {
            if (string.IsNullOrEmpty(_getPagedListSql))
                throw new Exception("GetListPage is not supported with the current SQL Dialect");

            if (pageNumber < 1)
                throw new Exception("Page must be greater than 0");

            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var name = GetTableName(currenttype);
            var sb = new StringBuilder();
            var query = _getPagedListSql;
            if (string.IsNullOrEmpty(orderby)) {
                orderby = GetColumnName(idProps.First());
            }

            //create a new empty instance of the type to get the base properties
            BuildSelect(sb, GetScaffoldableProperties<T>().ToArray());
            query = query.Replace("{SelectColumns}", sb.ToString());
            query = query.Replace("{TableName}", name);
            query = query.Replace("{PageNumber}", pageNumber.ToString());
            query = query.Replace("{RowsPerPage}", rowsPerPage.ToString());
            query = query.Replace("{OrderBy}", orderby);
            query = query.Replace("{WhereClause}", conditions);
            query = query.Replace("{Offset}", ((pageNumber - 1) * rowsPerPage).ToString());

            //if (Debugger.IsAttached) Trace.WriteLine($"GetListPaged<{currenttype}>: {query}");

            return _conn.QueryAsync<T>(query, parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction, commandType: commandType);
        }

        /// <summary>
        /// <para>Inserts a row into the database asynchronously</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</para>
        /// </summary>
        /// <param name="entityToInsert"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</returns>
        public virtual Task<int?> InsertAsync<TEntity>(TEntity entityToInsert, int? commandTimeout = null)
        {
            return InsertAsync<int?, TEntity>(entityToInsert, commandTimeout);
        }

        /// <summary>
        /// <para>Inserts a row into the database, using ONLY the properties defined by TEntity</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</para>
        /// </summary>
        /// <param name="entityToInsert"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public virtual async Task<TKey> InsertAsync<TKey, TEntity>(TEntity entityToInsert, int? commandTimeout = null)
        {
            var idProps = GetIdProperties(entityToInsert).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Insert<T> only supports an entity with a [Key] or Id property");

            var keyHasPredefinedValue = false;
            var baseType = typeof(TKey);
            var underlyingType = Nullable.GetUnderlyingType(baseType);
            var keytype = underlyingType ?? baseType;
            if (keytype != typeof(int) && keytype != typeof(uint) && keytype != typeof(long) &&
                keytype != typeof(ulong) && keytype != typeof(short) && keytype != typeof(ushort) &&
                keytype != typeof(Guid) && keytype != typeof(string)) {
                throw new Exception("Invalid return type");
            }

            var name = GetTableName(entityToInsert.GetType());
            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0}", name);
            sb.Append(" (");
            BuildInsertParameters<TEntity>(sb);
            sb.Append(") ");
            sb.Append("values");
            sb.Append(" (");
            BuildInsertValues<TEntity>(sb);
            sb.Append(")");

            if ((keytype == typeof(int) || keytype == typeof(long)) &&
                Convert.ToInt64(idProps.First().GetValue(entityToInsert, null)) == 0) {
                sb.Append(";" + _getIdentitySql);
            }
            else if (keytype == typeof(Guid)) {
                var guidvalue = (Guid) idProps.First().GetValue(entityToInsert, null);
                if (guidvalue == Guid.Empty) {
                    var newguid = SequentialGuid();
                    idProps.First().SetValue(entityToInsert, newguid, null);
                }
                else {
                    keyHasPredefinedValue = true;
                }

                sb.Append(";select '" + idProps.First().GetValue(entityToInsert, null) + "' as id");
            }
            else {
                keyHasPredefinedValue = true;
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"Insert: {sb}");

            if (keytype == typeof(Guid) || keyHasPredefinedValue) {
                await _conn.ExecuteAsync(sb.ToString(), entityToInsert,
                    commandTimeout: commandTimeout ?? DefaultCommandTimeout, transaction: _transaction);
                return (TKey) idProps.First().GetValue(entityToInsert, null);
            }

            var result = await _conn.QueryAsync(sb.ToString(), entityToInsert,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout, transaction: _transaction);
            return (TKey) result.First().id;
        }

        /// <summary>
        /// <para>Updates a record or records in the database with only the properties of TEntity</para>
        /// <para>By default updates records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Updates records where the Id property and properties with the [Key] attribute match those in the database.</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns number of rows affected</para>
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of affected records</returns>
        public virtual Task<int> UpdateAsync<T>(T entityToUpdate, int? commandTimeout = null)
        {
            var idProps = GetIdProperties(entityToUpdate).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = GetTableName(entityToUpdate.GetType());

            var sb = new StringBuilder();
            sb.AppendFormat("update {0}", name);

            sb.AppendFormat(" set ");
            BuildUpdateSet(entityToUpdate, sb);
            sb.Append(" where ");
            BuildWhere<T>(sb, idProps, entityToUpdate);

            //if (Debugger.IsAttached) Trace.WriteLine($"Update: {sb}");

            return _conn.ExecuteAsync(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>Deletes a record or records in the database that match the object passed in</para>
        /// <para>-By default deletes records in the table matching the class name</para>
        /// <para>Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the number of records affected</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityToDelete"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records affected</returns>
        public virtual Task<int> DeleteAsync<T>(T entityToDelete, int? commandTimeout = null)
        {
            var idProps = GetIdProperties(entityToDelete).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = GetTableName(entityToDelete.GetType());

            var sb = new StringBuilder();
            sb.AppendFormat("Delete from {0}", name);

            sb.Append(" where ");
            BuildWhere<T>(sb, idProps, entityToDelete);

            //if (Debugger.IsAttached) Trace.WriteLine($"Delete: {sb}");

            return _conn.ExecuteAsync(sb.ToString(), entityToDelete, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>Deletes a record or records in the database by ID</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where the Id property and properties with the [Key] attribute match those in the database</para>
        /// <para>The number of records affected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records affected</returns>
        public virtual Task<int> DeleteAsync<T>(object id, int? commandTimeout = null)
        {
            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Delete<T> only supports an entity with a [Key] or Id property");

            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            sb.AppendFormat("Delete from {0} where", name);
            sb.Append(GetWhereClauseFromIdColumns(idProps));

            var parameters = new DynamicParameters();
            if (idProps.Count == 1)
                parameters.Add("@" + idProps.First().Name, id);
            else {
                foreach (var prop in idProps)
                    parameters.Add("@" + prop.Name, id.GetType().GetProperty(prop.Name)?.GetValue(id, null));
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"Delete<{currenttype}> {sb}");

            return _conn.ExecuteAsync(sb.ToString(), parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>The number of records affected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records affected</returns>
        public virtual Task<int> DeleteCollectionAsync<T>(object whereConditions, int? commandTimeout = null)
        {
            var currenttype = typeof(T);
            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            var whereprops = GetAllProperties(whereConditions).ToArray();
            sb.AppendFormat("Delete from {0}", name);
            if (whereprops.Any()) {
                sb.Append(" where ");
                BuildWhere<T>(sb, whereprops);
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"DeleteList<{currenttype}> {sb}");

            return _conn.ExecuteAsync(sb.ToString(), whereConditions, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age"</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records affected</returns>
        public virtual Task<int> DeleteCollectionAsync<T>(string conditions, object parameters = null, int? commandTimeout = null)
        {
            if (string.IsNullOrEmpty(conditions))
                throw new ArgumentException("DeleteList<T> requires a where clause");
            if (!conditions.ToLower().Contains("where"))
                throw new ArgumentException("DeleteList<T> requires a where clause and must contain the WHERE keyword");

            var currenttype = typeof(T);
            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            sb.AppendFormat("Delete from {0}", name);
            sb.Append(" " + conditions);

            //if (Debugger.IsAttached) Trace.WriteLine($"DeleteList<{currenttype}> {sb}");

            return _conn.ExecuteAsync(sb.ToString(), parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        public virtual Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null)
        {
            return _conn.ExecuteAsync(query, parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        public virtual Task<int> ExecuteQueryAsync(string query, DbParameters parameters, int? commandTimeout = null)
        {
            return _conn.ExecuteAsync(query, parameters.DynamicParameters,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a number of records entity by a single id from table T</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>
        public virtual Task<int> GetRecordCountAsync<T>(string conditions = "", object parameters = null,
            int? commandTimeout = null)
        {
            var currenttype = typeof(T);
            var name = GetTableName(currenttype);
            var sb = new StringBuilder();
            sb.Append("Select count(1)");
            sb.AppendFormat(" from {0}", name);
            sb.Append(" " + conditions);

            //if (Debugger.IsAttached) Trace.WriteLine($"RecordCount<{currenttype}>: {sb}");

            return _conn.ExecuteScalarAsync<int>(sb.ToString(), parameters, commandTimeout: commandTimeout ?? DefaultCommandTimeout,
                transaction: _transaction);
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a number of records entity by a single id from table T</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>s
        public virtual Task<int> GetRecordCountAsync<T>(object whereConditions, int? commandTimeout = null)
        {
            var currenttype = typeof(T);
            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            var whereprops = GetAllProperties(whereConditions).ToArray();
            sb.Append("Select count(1)");
            sb.AppendFormat(" from {0}", name);

            if (whereprops.Any()) {
                sb.Append(" where ");
                BuildWhere<T>(sb, whereprops);
            }

            //if (Debugger.IsAttached) Trace.WriteLine($"RecordCount<{currenttype}>: {sb}");

            return _conn.ExecuteScalarAsync<int>(sb.ToString(), whereConditions,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout, transaction: _transaction);
        }

        #endregion

        #region Private Helpers

        //build update statement based on list on an entity
        private void BuildUpdateSet<T>(T entityToUpdate, StringBuilder sb)
        {
            var nonIdProps = GetUpdateableProperties(entityToUpdate).ToArray();

            for (var i = 0; i < nonIdProps.Length; i++) {
                var property = nonIdProps[i];

                sb.AppendFormat("{0} = @{1}", GetColumnName(property), property.Name);
                if (i < nonIdProps.Length - 1)
                    sb.AppendFormat(", ");
            }
        }

        //build select clause based on list of properties skipping ones with the IgnoreSelect and NotMapped attribute
        private void BuildSelect(StringBuilder sb, IEnumerable<PropertyInfo> props)
        {
            var propertyInfos = props as IList<PropertyInfo> ?? props.ToList();
            var addedAny = false;
            for (var i = 0; i < propertyInfos.Count(); i++) {
                var property = propertyInfos.ElementAt(i);

                if (property.GetCustomAttributes(true).Any(attr =>
                    attr.GetType().Name == typeof(IgnoreSelectAttribute).Name ||
                    attr.GetType().Name == typeof(NotMappedAttribute).Name)) continue;

                if (addedAny)
                    sb.Append(",");
                sb.Append(GetColumnName(property));
                //if there is a custom column name add an "as customcolumnname" to the item so it maps properly
                if (property.GetCustomAttributes(true)
                        .SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) != null)
                    sb.Append(" as " + Encapsulate(property.Name));
                addedAny = true;
            }
        }

        private void BuildWhere<TEntity>(StringBuilder sb, IEnumerable<PropertyInfo> idProps,
            object whereConditions = null)
        {
            var propertyInfos = idProps.ToArray();
            for (var i = 0; i < propertyInfos.Count(); i++) {
                var useIsNull = false;

                //match up generic properties to source entity properties to allow fetching of the column attribute
                //the anonymous object used for search doesn't have the custom attributes attached to them so this allows us to build the correct where clause
                //by converting the model type to the database column name via the column attribute
                var propertyToUse = propertyInfos.ElementAt(i);
                var sourceProperties = GetScaffoldableProperties<TEntity>().ToArray();
                for (var x = 0; x < sourceProperties.Count(); x++) {
                    if (sourceProperties.ElementAt(x).Name == propertyToUse.Name) {
                        if (whereConditions != null && propertyToUse.CanRead &&
                            (propertyToUse.GetValue(whereConditions, null) == null ||
                             propertyToUse.GetValue(whereConditions, null) == DBNull.Value)) {
                            useIsNull = true;
                        }

                        propertyToUse = sourceProperties.ElementAt(x);
                        break;
                    }
                }

                sb.AppendFormat(
                    useIsNull ? "{0} is null" : "{0} = @{1}",
                    GetColumnName(propertyToUse),
                    propertyToUse.Name);

                if (i < propertyInfos.Count() - 1)
                    sb.AppendFormat(" and ");
            }
        }

        //build insert values which include all properties in the class that are:
        //Not named Id
        //Not marked with the Editable(false) attribute
        //Not marked with the [Key] attribute (without required attribute)
        //Not marked with [IgnoreInsert]
        //Not marked with [NotMapped]
        private void BuildInsertValues<T>(StringBuilder sb)
        {
            var props = GetScaffoldableProperties<T>().ToArray();
            for (var i = 0; i < props.Count(); i++) {
                var property = props.ElementAt(i);
                if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(string)
                                                          && property.GetCustomAttributes(true).Any(attr =>
                                                              attr.GetType().Name == typeof(KeyAttribute).Name)
                                                          && property.GetCustomAttributes(true).All(attr =>
                                                              attr.GetType().Name != typeof(RequiredAttribute).Name))
                    continue;
                if (property.GetCustomAttributes(true).Any(attr =>
                    attr.GetType().Name == typeof(IgnoreInsertAttribute).Name ||
                    attr.GetType().Name == typeof(NotMappedAttribute).Name ||
                    attr.GetType().Name == typeof(ReadOnlyAttribute).Name && IsReadOnly(property))
                ) continue;

                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                    property.GetCustomAttributes(true)
                        .All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name) &&
                    property.PropertyType != typeof(Guid)) continue;

                sb.AppendFormat("@{0}", property.Name);
                if (i < props.Count() - 1)
                    sb.Append(", ");
            }

            if (sb.ToString().EndsWith(", "))
                sb.Remove(sb.Length - 2, 2);

        }

        //build insert parameters which include all properties in the class that are not:
        //marked with the Editable(false) attribute
        //marked with the [Key] attribute
        //marked with [IgnoreInsert]
        //named Id
        //marked with [NotMapped]
        private void BuildInsertParameters<T>(StringBuilder sb)
        {
            var props = GetScaffoldableProperties<T>().ToArray();

            for (var i = 0; i < props.Count(); i++) {
                var property = props.ElementAt(i);
                if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(string)
                                                          && property.GetCustomAttributes(true).Any(attr =>
                                                              attr.GetType().Name == typeof(KeyAttribute).Name)
                                                          && property.GetCustomAttributes(true).All(attr =>
                                                              attr.GetType().Name != typeof(RequiredAttribute).Name))
                    continue;
                if (property.GetCustomAttributes(true).Any(attr =>
                    attr.GetType().Name == typeof(IgnoreInsertAttribute).Name ||
                    attr.GetType().Name == typeof(NotMappedAttribute).Name ||
                    attr.GetType().Name == typeof(ReadOnlyAttribute).Name && IsReadOnly(property))) continue;

                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                    property.GetCustomAttributes(true)
                        .All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name) &&
                    property.PropertyType != typeof(Guid)) continue;

                sb.Append(GetColumnName(property));
                if (i < props.Count() - 1)
                    sb.Append(", ");
            }

            if (sb.ToString().EndsWith(", "))
                sb.Remove(sb.Length - 2, 2);
        }

        //Get all properties in an entity
        private IEnumerable<PropertyInfo> GetAllProperties<T>(T entity) where T : class
        {
            if (entity == null) return new PropertyInfo[0];
            return entity.GetType().GetProperties();
        }

        //Get all properties that are not decorated with the Editable(false) attribute
        private IEnumerable<PropertyInfo> GetScaffoldableProperties<T>()
        {
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties();

            props = props.Where(p =>
                p.GetCustomAttributes(true).Any(attr =>
                    attr.GetType().Name == typeof(EditableAttribute).Name && !IsEditable(p)) == false);


            return props.Where(p => p.PropertyType.IsSimpleType() || IsEditable(p));
        }

        //Determine if the Attriute has an AllowEdit key and return its boolean state
        //fake the funk and try to mimic EditableAttribute in System.ComponentModel.DataAnnotations 
        //This allows use of the DataAnnotations property in the model and have the SimpleCRUD engine just figure it out without a reference
        private bool IsEditable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0) {
                dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == typeof(EditableAttribute).Name);
                if (write != null) {
                    return write.AllowEdit;
                }
            }

            return false;
        }

        //Determine if the Attribute has an IsReadOnly key and return its boolean state
        //fake the funk and try to mimic ReadOnlyAttribute in System.ComponentModel 
        //This allows use of the DataAnnotations property in the model and have the SimpleCRUD engine just figure it out without a reference
        private bool IsReadOnly(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0) {
                dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == typeof(ReadOnlyAttribute).Name);
                if (write != null) {
                    return write.IsReadOnly;
                }
            }

            return false;
        }

        //Get all properties that are:
        //Not named Id
        //Not marked with the Key attribute
        //Not marked ReadOnly
        //Not marked IgnoreInsert
        //Not marked NotMapped
        private IEnumerable<PropertyInfo> GetUpdateableProperties<T>(T entity)
        {
            var updateableProperties = GetScaffoldableProperties<T>();
            //remove ones with ID
            updateableProperties =
                updateableProperties.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            //remove ones with key attribute
            updateableProperties = updateableProperties.Where(p =>
                p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name) == false);
            //remove ones that are readonly
            updateableProperties = updateableProperties.Where(p =>
                p.GetCustomAttributes(true).Any(attr =>
                    (attr.GetType().Name == typeof(ReadOnlyAttribute).Name) && IsReadOnly(p)) == false);
            //remove ones with IgnoreUpdate attribute
            updateableProperties = updateableProperties.Where(p =>
                p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(IgnoreUpdateAttribute).Name) ==
                false);
            //remove ones that are not mapped
            updateableProperties = updateableProperties.Where(p =>
                p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(NotMappedAttribute).Name) ==
                false);

            return updateableProperties;
        }

        //Get all properties that are named Id or have the Key attribute
        //For Inserts and updates we have a whole entity so this method is used
        private IEnumerable<PropertyInfo> GetIdProperties(object entity)
        {
            var type = entity.GetType();
            return GetIdProperties(type);
        }

        //Gets the table name for this type
        //For Get(id) and Delete(id) we don't have an entity, just the type so this method is used
        //Use dynamic type to be able to handle both our Table-attribute and the DataAnnotation
        //Uses class name by default and overrides if the class has a Table attribute
        public string GetTableName(Type type)
        {
            if (_tableNames.TryGetValue(type, out var tableName))
                return tableName;

            tableName = _tableNameResolver.ResolveTableName(type);

            _tableNames.AddOrUpdate(type, tableName, (t, v) => tableName);

            return tableName;
        }

        private string GetColumnName(PropertyInfo propertyInfo)
        {
            string columnName, key = string.Format("{0}.{1}", propertyInfo.DeclaringType, propertyInfo.Name);

            if (_columnNames.TryGetValue(key, out columnName))
                return columnName;

            columnName = _columnNameResolver.ResolveColumnName(propertyInfo);

            _columnNames.AddOrUpdate(key, columnName, (t, v) => columnName);

            return columnName;
        }

        #endregion

        //Get all properties that are named Id or have the Key attribute
        //For Get(id) and Delete(id) we don't have an entity, just the type so this method is used
        public IEnumerable<PropertyInfo> GetIdProperties(Type type)
        {
            var tp = type.GetProperties().Where(p =>
                p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).ToList();
            return tp.Any()
                ? tp
                : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }

        public string GetWhereClauseFromIdColumns(IEnumerable<PropertyInfo> idProperties)
        {
            var idProps = idProperties.ToList();
            var sb = new StringBuilder();

            if (idProps.Count > 0) {
                for (var i = 0; i < idProps.Count; i++) {
                    if (i > 0)
                        sb.Append(" and ");
                    sb.AppendFormat("{0} = @{1}", GetColumnName(idProps[i]), idProps[i].Name);
                }
            }

            return sb.ToString();
        }

        public string Encapsulate(string databaseword)
        {
            return string.Format(_encapsulation, databaseword);
        }

        /// <summary>
        /// Generates a guid based on the current date/time
        /// http://stackoverflow.com/questions/1752004/sequential-guid-generator-c-sharp
        /// </summary>
        /// <returns></returns>
        public Guid SequentialGuid()
        {
            var tempGuid = Guid.NewGuid();
            var bytes = tempGuid.ToByteArray();
            var time = DateTime.Now;
            bytes[3] = (byte) time.Year;
            bytes[2] = (byte) time.Month;
            bytes[1] = (byte) time.Day;
            bytes[0] = (byte) time.Hour;
            bytes[5] = (byte) time.Minute;
            bytes[4] = (byte) time.Second;
            return new Guid(bytes);
        }

        /// <summary>
        /// Database server dialects
        /// </summary>
        public enum Dialect
        {
            SqlServer,
            PostgreSql,
            SqLite,
            MySql,
        }

        /// <summary>
        /// Sets the table name resolver
        /// </summary>
        /// <param name="resolver">The resolver to use when requesting the format of a table name</param>
        public void SetTableNameResolver(ITableNameResolver resolver)
        {
            _tableNameResolver = resolver;
        }

        /// <summary>
        /// Sets the column name resolver
        /// </summary>
        /// <param name="resolver">The resolver to use when requesting the format of a column name</param>
        public void SetColumnNameResolver(IColumnNameResolver resolver)
        {
            _columnNameResolver = resolver;
        }

        public interface ITableNameResolver
        {
            string ResolveTableName(Type type);
        }

        public interface IColumnNameResolver
        {
            string ResolveColumnName(PropertyInfo propertyInfo);
        }

        public class TableNameResolver : ITableNameResolver
        {
            private readonly string _encapsulation;

            public TableNameResolver(string encapsulation)
            {
                _encapsulation = encapsulation;
            }

            public virtual string ResolveTableName(Type type)
            {
                var tableName = Encapsulate(type.Name);

                var tableattr = type.GetCustomAttributes(true)
                    .SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
                if (tableattr != null) {
                    tableName = Encapsulate(tableattr.Name);
                    try {
                        if (!string.IsNullOrEmpty(tableattr.Schema)) {
                            string schemaName = Encapsulate(tableattr.Schema);
                            tableName = $"{schemaName}.{tableName}";
                        }
                    }
                    catch (RuntimeBinderException) {
                        //Schema doesn't exist on this attribute.
                    }
                }

                return tableName;
            }

            private string Encapsulate(string databaseword)
            {
                return string.Format(_encapsulation, databaseword);
            }
        }

        public class ColumnNameResolver : IColumnNameResolver
        {
            private readonly string _encapsulation;

            public ColumnNameResolver(string encapsulation)
            {
                _encapsulation = encapsulation;
            }

            public virtual string ResolveColumnName(PropertyInfo propertyInfo)
            {
                var columnName = Encapsulate(propertyInfo.Name);

                var columnattr = propertyInfo.GetCustomAttributes(true)
                    .SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) as dynamic;
                if (columnattr != null) {
                    columnName = Encapsulate(columnattr.Name);
                    //if (Debugger.IsAttached) Trace.WriteLine($"Column name for type overridden from {propertyInfo.Name} to {columnName}");
                }

                return columnName;
            }

            private string Encapsulate(string databaseword)
            {
                return string.Format(_encapsulation, databaseword);
            }
        }
    }

    /// <summary>
    /// Optional Table attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Optional Table attribute.
        /// </summary>
        /// <param name="tableName"></param>
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        /// <summary>
        /// Name of the table
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Name of the schema
        /// </summary>
        public string Schema { get; set; }
    }

    /// <summary>
    /// Optional Column attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Optional Column attribute.
        /// </summary>
        /// <param name="columnName"></param>
        public ColumnAttribute(string columnName)
        {
            Name = columnName;
        }

        /// <summary>
        /// Name of the column
        /// </summary>
        public string Name { get; private set; }
    }

    /// <summary>
    /// Optional Key attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the Primary Key of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional NotMapped attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify that the property is not mapped
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotMappedAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional Key attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify a required property of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional Editable attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the properties that are editable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableAttribute : Attribute
    {
        /// <summary>
        /// Optional Editable attribute.
        /// </summary>
        /// <param name="iseditable"></param>
        public EditableAttribute(bool iseditable)
        {
            AllowEdit = iseditable;
        }

        /// <summary>
        /// Does this property persist to the database?
        /// </summary>
        public bool AllowEdit { get; private set; }
    }

    /// <summary>
    /// Optional Readonly attribute.
    /// You can use the System.ComponentModel version in its place to specify the properties that are editable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ReadOnlyAttribute : Attribute
    {
        /// <summary>
        /// Optional ReadOnly attribute.
        /// </summary>
        /// <param name="isReadOnly"></param>
        public ReadOnlyAttribute(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Does this property persist to the database?
        /// </summary>
        public bool IsReadOnly { get; private set; }
    }

    /// <summary>
    /// Optional IgnoreSelect attribute.
    /// Custom for Dapper.SimpleCRUD to exclude a property from Select methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreSelectAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional IgnoreInsert attribute.
    /// Custom for Dapper.SimpleCRUD to exclude a property from Insert methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreInsertAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional IgnoreUpdate attribute.
    /// Custom for Dapper.SimpleCRUD to exclude a property from Update methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreUpdateAttribute : Attribute
    {
    }

    internal static class TypeExtension
    {
        //You can't insert or update complex types. Lets filter them out.
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type> {
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(string),
                typeof(char),
                typeof(Guid),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(byte[])
            };
            return simpleTypes.Contains(type) || type.IsEnum;
        }
    }
}