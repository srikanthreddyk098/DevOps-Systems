using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.LogAnalytics.Models;
using CAD.DataCollector.LogAnalytics.Repositories;
using Microsoft.Azure.OperationalInsights;
using Microsoft.Azure.OperationalInsights.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace CAD.DataCollector.LogAnalytics
{
    public class LogAnalyticsDataCollector : BackgroundService
    {
        private readonly ILogger<LogAnalyticsDataCollector> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly LogAnalyticsQueryRepository _logAnalyticsQueryRepository;
        private readonly List<Tuple<LogAnalyticsQueryModel, CancellationTokenSource>> _tupleOfRunningQueries;
        private CancellationToken _cancellationToken;

        public LogAnalyticsDataCollector(ILogger<LogAnalyticsDataCollector> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _connectionString = configuration["connectionString"];
            _logAnalyticsQueryRepository = new LogAnalyticsQueryRepository(_connectionString);
            _tupleOfRunningQueries = new List<Tuple<LogAnalyticsQueryModel, CancellationTokenSource>>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            try {
                while (true) {
                    if (_cancellationToken.IsCancellationRequested) {
                        CancelAllRunningQueries();
                        _logger.LogDebug("Cancellation request was found. Returning...");
                    }

                    var queries = (await _logAnalyticsQueryRepository.GetAllAsync(new {IsDisabled = 0})).ToList();

                    //do not process queries that are already running
                    var alreadyRunningQueries = _tupleOfRunningQueries.Where(x => queries.Any(q => q.IsEqual(x.Item1))).ToList();
                    foreach (var alreadyRunningQuery in alreadyRunningQueries) {
                        queries.RemoveAt(queries.FindIndex(x => x.IsEqual(alreadyRunningQuery.Item1)));
                    }

                    //cancel and remove running queries that are outdated
                    foreach (var alreadyRunningQuery in _tupleOfRunningQueries.Where(x => !alreadyRunningQueries.Any(y => y.Equals(x)))) {
                        alreadyRunningQuery.Item2.Cancel();
                    }
                    _tupleOfRunningQueries.RemoveAll(x => x.Item2.IsCancellationRequested);

                    foreach (var query in queries) {
                        if (_cancellationToken.IsCancellationRequested) {
                            CancelAllRunningQueries();
                            _logger.LogDebug("Cancellation request was found. Exiting...");
                            return;
                        }

                        var queryCancellationToken = new CancellationTokenSource();

                        var task = Task.Factory.StartNew(function: async () =>
                        {
                            while (!_cancellationToken.IsCancellationRequested && !queryCancellationToken.Token.IsCancellationRequested) {
                                try {
                                    var client = new OperationalInsightsDataClient(await GetOperationInsightsCredentials(query)) {
                                        WorkspaceId = query.WorkspaceId
                                    };
                                    var results = await client.QueryAsync(query.Query, cancellationToken: _cancellationToken);
                                    await ProcessQueryResultsAsync(results, query);
                                }
                                catch (TaskCanceledException) {
                                    _logger.LogDebug("Task.Delay threw a task cancelled exception.");
                                    continue;
                                }
                                catch (Exception ex) {
                                    _logger.LogError(ex, $"An exception occurred running log analytics query with query: {query}");
                                }

                                await Task.Delay(query.RunFrequencyInMin * 60000, queryCancellationToken.Token);
                            }
                        }, queryCancellationToken.Token);

                        _tupleOfRunningQueries.Add(new Tuple<LogAnalyticsQueryModel, CancellationTokenSource>(query, queryCancellationToken));
                    }

                    await Task.Delay(300000, _cancellationToken);
                }
            }
            catch (Exception ex) {
                _logger.LogCritical(ex, "An unexpected exception occurred. Exiting main thread.");
            }
        }

        private void CancelAllRunningQueries() 
        {
            foreach (var tuple in _tupleOfRunningQueries) {
                tuple.Item2.Cancel();
            }
        }

        private async Task<ServiceClientCredentials> GetOperationInsightsCredentials(LogAnalyticsQueryModel query)
        {
            var domain = query.Domain;
            var clientId = _configuration[query.ClientId];
            var clientSecret = _configuration[query.ClientSecret];
            var authEndpoint = "https://login.microsoftonline.com";
            var tokenAudience = "https://api.loganalytics.io/";

            var adSettings = new ActiveDirectoryServiceSettings {
                AuthenticationEndpoint = new Uri(authEndpoint),
                TokenAudience = new Uri(tokenAudience),
                ValidateAuthority = true
            };

            return await ApplicationTokenProvider.LoginSilentAsync(domain, clientId, clientSecret, adSettings);
        }

        private async Task ProcessQueryResultsAsync(QueryResults results, LogAnalyticsQueryModel query)
        {
            var sqlQueries = new List<string>();

                var columns = results.Tables[0].Columns;
                foreach (var row in results.Tables[0].Rows) {
                    if (_cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {query.TableName} ");
                    sb.Append("(");
                    sb.Append("TenantId,WorkspaceId,");
                    sb.Append(GetTableColumns(GetFilteredColumns(columns)));
                    sb.Append(") ");
                    sb.Append("VALUES");
                    sb.Append("(");
                    sb.Append($"'{query.TenantId}','{query.WorkspaceId}',");
                    sb.Append(GetRowValues(row, columns));
                    sb.Append(");");

                    sqlQueries.Add(sb.ToString());
                }

            if (!sqlQueries.Any()) return;
            if (!await VerifyTableAndColumnsExist(query.TableName, results.Tables[0].Columns)) return;

            try {
                using var db = new DbConnectionWithRetry(new SqlConnection(_connectionString));
                db.BeginTransaction();

                if (query.TruncateEveryRun) {
                    await db.ExecuteQueryAsync($"DELETE FROM {query.TableName} WHERE TenantId = @TenantId AND WorkspaceId = @WorkspaceId",
                        new {query.TableName, query.TenantId, query.WorkspaceId});
                }

                foreach (var sqlQuery in sqlQueries) {
                    if (_cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    try {
                        var result = await db.ExecuteQueryAsync(sqlQuery);
                        if (result != 1) {
                            _logger.LogWarning($"The following sql query did not execute successfully: {sqlQuery}");
                        }
                    }
                    catch (Exception ex) {
                        _logger.LogError($"An exception occurred executing the following sql query: {sqlQuery}", ex);
                        throw;
                    }
                }

                db.CommitTransaction();
            }
            catch (Exception ex) {
                ;
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
            sb.Append("[TenantId] VARCHAR(50) NOT NULL,");
            sb.Append("[WorkspaceId] VARCHAR(50) NOT NULL,");
            foreach (var column in GetFilteredColumns(columns)) {
                sb.Append($"[{column.Name}] {GetColumnSqlType(column.Type)} NULL,");
            }

            return sb.Remove(sb.Length - 1, 1).Append(")").ToString();
        }

        private IEnumerable<Column> GetFilteredColumns(IEnumerable<Column> columns)
        {
            return columns.Where(x =>
                !string.Equals(x.Name, "TenantId", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(x.Name, "WorkspaceId", StringComparison.OrdinalIgnoreCase));
        }

        private string GenerateColumnCheckQuery(string queryTableName, IList<Column> columns)
        {
            var sb = new StringBuilder();
            columns.Add(new Column("TenantId", "string"));
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

        private string GetTableColumns(IEnumerable<Column> columns)
        {
            var sb = new StringBuilder();
            foreach (var column in columns) {
                sb.Append(column.Name);
                sb.Append(",");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        private string GetRowValues(IList<string> row, IList<Column> columns)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < row.Count; i++) {
                if (string.Equals(columns[i].Name, "TenantId", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(columns[i].Name, "WorkspaceId", StringComparison.OrdinalIgnoreCase)) continue;
                if (row[i] == null) {
                    sb.Append("null");
                    sb.Append(",");
                }
                else if (columns[i].Type.Equals("string", StringComparison.OrdinalIgnoreCase) ||
                         columns[i].Type.Equals("datetime", StringComparison.OrdinalIgnoreCase)) {
                    sb.Append(string.IsNullOrEmpty(row[i]) ? "NULL" : $"'{row[i].Replace("'", "''")}'");
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
