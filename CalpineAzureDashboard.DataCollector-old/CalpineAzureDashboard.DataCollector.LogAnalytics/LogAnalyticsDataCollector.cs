using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureDashboard.Data;
using CalpineAzureDashboard.Data.Repository;
using log4net;
using Microsoft.Azure.OperationalInsights;
using Microsoft.Azure.OperationalInsights.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace CalpineAzureDashboard.DataCollector.LogAnalytics
{
    public class LogAnalyticsDataCollector
    {
        private readonly CancellationToken _cancellationToken;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _connectionString;
        private readonly LogAnalyticsQueryRepository<LogAnalyticsQueryModel> _repository;
        private readonly List<Tuple<int, LogAnalyticsQueryModel, CancellationTokenSource>> _tupleOfRunningQueries;

        private static readonly ILog Log = LogManager.GetLogger(typeof(LogAnalyticsDataCollector));

        public LogAnalyticsDataCollector(CancellationToken cancellationToken, string clientId, string clientSecret, string connectionString)
        {
            _cancellationToken = cancellationToken;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _connectionString = connectionString;

            _repository = new LogAnalyticsQueryRepository<LogAnalyticsQueryModel>(connectionString);
            _tupleOfRunningQueries = new List<Tuple<int, LogAnalyticsQueryModel, CancellationTokenSource>>();
        }

        public async Task StartLogAnalyticsDataCollector()
        {
            while (true) {
                if (_cancellationToken.IsCancellationRequested) {
                    CancelAllRunningQueries();
                    Log.Debug("Cancellation request was found. Returning...");
                }

                var queries = await _repository.GetCollectionAsync(new {IsDisabled = 0});
                foreach (var query in queries) {
                    if (_cancellationToken.IsCancellationRequested) {
                        CancelAllRunningQueries();
                        Log.Debug("Cancellation request was found. Returning...");
                        return;
                    }

                    var alreadyRunningQueryTuple = _tupleOfRunningQueries.FirstOrDefault(x => x.Item1.Equals(query.Id));
                    if (alreadyRunningQueryTuple != null) {
                        if (!alreadyRunningQueryTuple.Item2.IsEqual(query)) {
                            alreadyRunningQueryTuple.Item3.Cancel();
                            _tupleOfRunningQueries.Remove(alreadyRunningQueryTuple);
                        }
                        else {
                            continue;
                        }
                    }

                    var cancellationTokenSource = new CancellationTokenSource();

                    var task = Task.Factory.StartNew(async () =>
                    {
                        while (!_cancellationToken.IsCancellationRequested && !cancellationTokenSource.Token.IsCancellationRequested) {
                            try {
                                var results = await RunLogAnalyticsQueryAsync(query);
                                await ProcessQueryResultsAsync(results, query);
                            }
                            catch (TaskCanceledException) {
                                Log.Debug("Task.Delay threw a task cancelled exception.");
                                continue;
                            }
                            catch (Exception ex) {
                                Log.Error($"An exception occurred running log analytics query with id: {query.Id}", ex);
                            }

                            await Task.Delay(query.RunFrequencyInMin * 60000, _cancellationToken);
                        }
                    }, _cancellationToken);

                    _tupleOfRunningQueries.Add(new Tuple<int, LogAnalyticsQueryModel, CancellationTokenSource>(query.Id, query, cancellationTokenSource));
                }

                try {
                    await Task.Delay(300000, _cancellationToken);
                }
                catch (TaskCanceledException) {
                    Log.Error("Task.Delay threw a task cancelled exception.");
                }
            }
        }

        private void CancelAllRunningQueries()
        {
            foreach (var tuple in _tupleOfRunningQueries) {
                tuple.Item3.Cancel();
            }
        }

        private async Task<Dictionary<string, QueryResults>> RunLogAnalyticsQueryAsync(LogAnalyticsQueryModel query)
        {
            var client = new OperationalInsightsDataClient(await GetOperationInsightsCredentials());
            var workspaceIds = query.WorkspaceId.Split(',');
            var results = new Dictionary<string, QueryResults>();
            foreach (var workspaceId in workspaceIds) {
                client.WorkspaceId = workspaceId;
                results.Add(workspaceId, await client.QueryAsync(query.Query, cancellationToken: _cancellationToken));
            }

            return results;
        }

        private async Task<ServiceClientCredentials> GetOperationInsightsCredentials()
        {
            var domain = "cpncorp.onmicrosoft.com";
            var authEndpoint = "https://login.microsoftonline.com";
            var tokenAudience = "https://api.loganalytics.io/";

            var adSettings = new ActiveDirectoryServiceSettings {
                AuthenticationEndpoint = new Uri(authEndpoint),
                TokenAudience = new Uri(tokenAudience),
                ValidateAuthority = true
            };

            return await ApplicationTokenProvider.LoginSilentAsync(domain, _clientId, _clientSecret, adSettings);
        }

        private async Task ProcessQueryResultsAsync(Dictionary<string, QueryResults> results, LogAnalyticsQueryModel query)
        {
            var sqlQueries = new List<string>();;

            foreach (var result in results) {
                var columns = result.Value.Tables[0].Columns;
                foreach (var row in result.Value.Tables[0].Rows) {
                    if (_cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {query.TableName} ");
                    sb.Append("(");
                    sb.Append(GetTableColumns(columns));
                    sb.Append(") ");
                    sb.Append("VALUES");
                    sb.Append("(");
                    sb.Append(GetRowValues(row, columns, result.Key));
                    sb.Append(");");

                    sqlQueries.Add(sb.ToString());
                }
            }

            if (!sqlQueries.Any()) return;
            if (!await VerifyTableAndColumnsExist(query.TableName, results.First().Value.Tables[0].Columns)) return;

            try {
                using (var db = new DbConnectionWithRetry(new SqlConnection(_connectionString))) {
                    db.BeginTransaction();

                    if (query.TruncateEveryRun) {
                        await db.ExecuteQueryAsync($"TRUNCATE TABLE {query.TableName};");
                    }

                    foreach (var sqlQuery in sqlQueries) {
                        if (_cancellationToken.IsCancellationRequested) {
                            return;
                        }

                        try {
                            var result = await db.ExecuteQueryAsync(sqlQuery);
                            if (result != 1) {
                                Log.Warn($"The following sql query did not execute successfully: {sqlQuery}");
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred executing the following sql query: {sqlQuery}", ex);
                            throw;
                        }
                    }

                    db.CommitTransaction();
                }
            }
            catch (Exception) {
                // ignored
            }
        }

        private async Task<bool> VerifyTableAndColumnsExist(string tableName, IList<Column> columns)
        {
            using (var db = new DbConnectionWithRetry(new SqlConnection(_connectionString))) {
                var sqlQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = 'CalpineAzureDashboard' AND TABLE_NAME = '{tableName}'";
                var tableExists = await db.GetAsync<int>(sqlQuery) > 0;

                if (!tableExists) {
                    var createTableQuery = GenerateCreateTableQuery(tableName, columns);
                    await db.ExecuteQueryAsync(createTableQuery);
                    return await db.GetAsync<int>(sqlQuery) > 0;
                }

                var columnCheckQuery = GenerateColumnCheckQuery(tableName, columns);
                await db.ExecuteQueryAsync(columnCheckQuery);
                return true;
            }
        }

        private string GenerateCreateTableQuery(string tableName, IList<Column> columns)
        {
            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE [dbo].[{tableName}](");

            sb.Append("[WorkspaceId] VARCHAR(50) NOT NULL,");

            foreach (var column in columns) {
                sb.Append($"[{column.Name}] {GetColumnSqlType(column.Type)} NULL,");
            }

            return sb.Remove(sb.Length - 1, 1).Append(")").ToString();
        }

        private string GenerateColumnCheckQuery(string queryTableName, IList<Column> columns)
        {
            var sb = new StringBuilder();
            columns.Add(new Column("WorkspaceId", "string"));
            foreach (var column in columns) {
                sb.Append($"IF COL_LENGTH('dbo.{queryTableName}', '{column.Name}') IS NULL");
                sb.Append(Environment.NewLine);
                sb.Append("BEGIN");
                sb.Append(Environment.NewLine);
                sb.Append($"ALTER TABLE dbo.{queryTableName} ADD [{column.Name}] {GetColumnSqlType(column.Type)} NULL");
                sb.Append(Environment.NewLine);
                sb.Append("END");
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private string GetColumnSqlType(string columnType)
        {
            switch (columnType) {
                case "string":
                {
                    return "[varchar](255)";
                }

                case "datetime":
                {
                    return "[datetime2](7)";
                }

                case "real":
                {
                    return "[real]";
                }

                case "int":
                {
                    return "[int]";
                }

                case "bool":
                {
                    return "[bit]";
                }

                default:
                {
                    throw new Exception($"Unsupported column type found: {columnType}");
                }
            }
        }

        private string GetTableColumns(IList<Column> columns)
        {
            var sb = new StringBuilder();
            sb.Append("WorkspaceId,");
            foreach (var column in columns) {
                sb.Append(column.Name);
                sb.Append(",");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        private string GetRowValues(IList<string> row, IList<Column> columns, string workspaceId)
        {
            var sb = new StringBuilder();
            sb.Append("'" + workspaceId + "',");
            for (int i = 0; i < row.Count; i++) {
                if (row[i] == null) {
                    sb.Append("null");
                    sb.Append(",");
                }
                else if (columns[i].Type.Equals("string", StringComparison.OrdinalIgnoreCase) ||
                         columns[i].Type.Equals("datetime", StringComparison.OrdinalIgnoreCase)) {
                    sb.Append("'");
                    sb.Append(row[i].Replace("'", "''"));
                    sb.Append("'");
                    sb.Append(",");
                }
                else if (columns[i].Type.Equals("bool", StringComparison.OrdinalIgnoreCase)) {
                    if (row[i].Equals("true", StringComparison.OrdinalIgnoreCase)) {
                        sb.Append(1);
                    }
                    else if (row[i].Equals("false", StringComparison.OrdinalIgnoreCase)) {
                        sb.Append(0);
                    }
                    else sb.Append("null");

                    sb.Append(",");
                }
                else {
                    sb.Append(row[i]);
                    sb.Append(",");
                }
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
