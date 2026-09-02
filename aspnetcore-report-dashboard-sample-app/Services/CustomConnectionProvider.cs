using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;
using Newtonsoft.Json.Linq;

namespace AspNetCoreReportDashboardSampleApp.Services {
    public class CustomConnectionProvider :
        IDataSourceWizardConnectionStringsProvider,
        IConnectionProviderService,
        IJsonDataConnectionProviderService,
        IDataSourceWizardJsonConnectionStorage {
        private readonly string _dataModelsFile;
        private readonly Dictionary<string, DataConnectionParametersBase> _connections =
            new Dictionary<string, DataConnectionParametersBase>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<CustomConnectionProvider>? _logger;

        public CustomConnectionProvider(IWebHostEnvironment env, ILogger<CustomConnectionProvider> logger)
            : this(env.ContentRootPath, logger) { }

        public CustomConnectionProvider(string contentRootPath, ILogger<CustomConnectionProvider>? logger = null) {
            _logger = logger;
            _dataModelsFile = Path.Combine(contentRootPath, "DataModels", "data-models.json");
            LoadConnections();
        }

        private void LoadConnections() {
            if(!File.Exists(_dataModelsFile)) {
                _logger?.LogInformation("Data models file not found at '{Path}'. No connections loaded.", _dataModelsFile);
                return;
            }

            try {
                var content = File.ReadAllText(_dataModelsFile);
                var models = JArray.Parse(content);

                foreach(var model in models) {
                    string? name = model["Name"]?.ToString();
                    var connInfo = model["ConnectionInfo"];
                    if(!string.IsNullOrEmpty(name) && connInfo != null) {
                        var parameters = CreateConnectionParameters(name, connInfo);
                        if(parameters != null)
                            _connections[name] = parameters;
                        else
                            _logger?.LogWarning("Could not map connection parameters for data model '{Name}'.", name);
                    }
                }

                _logger?.LogInformation("Loaded {Count} data connection(s) from '{Path}'.", _connections.Count, _dataModelsFile);
            } catch(Exception ex) {
                _logger?.LogError(ex, "Error loading data model connections from '{Path}'.", _dataModelsFile);
            }
        }

        private DataConnectionParametersBase? CreateConnectionParameters(string name, JToken connInfo) {
            string dbType = connInfo["databaseType"]?.ToString()
                         ?? connInfo["providerType"]?.ToString()
                         ?? string.Empty;
            string serverName = connInfo["serverName"]?.ToString() ?? string.Empty;
            string databaseName = connInfo["databaseName"]?.ToString()
                               ?? connInfo["database"]?.ToString() ?? string.Empty;
            string userName = connInfo["userName"]?.ToString() ?? string.Empty;
            string password = connInfo["password"]?.ToString() ?? string.Empty;
            bool integratedSecurity =
                connInfo["IsIntegratedSecurity"]?.ToObject<bool>() ??
                (connInfo["authenticationType"]?.ToString() == "Windows");

            if(dbType.Equals("MSSQLServer", StringComparison.OrdinalIgnoreCase)) {
                var p = new MsSqlConnectionParameters {
                    ServerName = serverName,
                    DatabaseName = databaseName,
                    AuthorizationType = integratedSecurity
                        ? MsSqlAuthorizationType.Windows
                        : MsSqlAuthorizationType.SqlServer,
                    TrustServerCertificate = DevExpress.Utils.DefaultBoolean.True
                };
                if(!integratedSecurity) {
                    p.UserName = userName;
                    p.Password = password;
                }
                return p;
            }

            if(dbType.Equals("Json", StringComparison.OrdinalIgnoreCase) ||
                dbType.Equals("JsonSource", StringComparison.OrdinalIgnoreCase)) {
                string? jsonDataSourceStr = connInfo["jsonDataSource"]?.ToString();
                if(!string.IsNullOrEmpty(jsonDataSourceStr)) {
                    try {
                        var jsonModel = JObject.Parse(jsonDataSourceStr);
                        var sourceToken = jsonModel["Source"];
                        if(sourceToken != null) {
                            string? sourceType = sourceToken["@SourceType"]?.ToString();
                            if(sourceType == "DevExpress.DataAccess.Json.UriJsonSource") {
                                string? uri = sourceToken["@Uri"]?.ToString();
                                if(uri != null)
                                    return new JsonSourceConnectionParameters { JsonSource = new UriJsonSource(new Uri(uri)) };
                            } else if(sourceType == "DevExpress.DataAccess.Json.CustomJsonSource") {
                                string? json = sourceToken["@Json"]?.ToString();
                                if(json != null)
                                    return new JsonSourceConnectionParameters { JsonSource = new CustomJsonSource { Json = json } };
                            }
                        }
                    } catch(Exception ex) {
                        _logger?.LogDebug(ex, "Failed to parse jsonDataSource field for connection '{Name}'; falling back to ConnectionString.", name);
                    }
                }

                string? jsonConnStr = connInfo["ConnectionString"]?.ToString();
                if(!string.IsNullOrEmpty(jsonConnStr))
                    return new CustomStringConnectionParameters(jsonConnStr);
            }

            string? customConnStr = connInfo["ConnectionString"]?.ToString();
            if(!string.IsNullOrEmpty(customConnStr))
                return new CustomStringConnectionParameters(customConnStr);

            return null;
        }

        public Dictionary<string, string> GetConnectionDescriptions() {
            var sqlConnections = _connections.Where(c => c.Value is MsSqlConnectionParameters)
                .ToDictionary(c => c.Key, c => c.Key);
            return sqlConnections;
        }

        public Dictionary<string, string> GetJsonConnectionDescriptions() {
            var jsonConnections = _connections.Where(c => c.Value is CustomStringConnectionParameters || c.Value is JsonSourceConnectionParameters)
                .ToDictionary(c => c.Key, c => c.Key);
            return jsonConnections;
        }

        public IEnumerable<JsonDataConnection> GetConnections() {
            var jsonConnections = _connections.Where(c => c.Value is CustomStringConnectionParameters || c.Value is JsonSourceConnectionParameters);
            foreach(var c in jsonConnections) {
                var conn = GetJsonDataConnection(c.Key);
                if (conn != null) yield return conn;
            }
        }

        public bool ContainsConnection(string connectionName) {
            return _connections.TryGetValue(connectionName, out var p) && (p is CustomStringConnectionParameters || p is JsonSourceConnectionParameters);
        }

        public void SaveConnection(string connectionName, JsonDataConnection connection, bool saveCredentials) {
            throw new NotSupportedException();
        }

        public bool CanSaveConnection => false;

        public DataConnectionParametersBase GetDataConnectionParameters(string name) {
            if(_connections.TryGetValue(name, out var connection))
                return connection;
            throw new KeyNotFoundException($"Connection '{name}' was not found in data-models.json.");
        }

        public DevExpress.DataAccess.Sql.SqlDataConnection? LoadConnection(string connectionName) {
            if(_connections.TryGetValue(connectionName, out var parameters))
                return new SqlDataConnection(connectionName, parameters);

            _logger?.LogWarning("SQL connection '{Name}' was not found in data-models.json.", connectionName);
            return null;
        }

        public JsonDataConnection? GetJsonDataConnection(string connectionName) {
            if(!_connections.TryGetValue(connectionName, out var parameters))
                return null;

            if(parameters is CustomStringConnectionParameters customParams)
                return new JsonDataConnection(customParams.ConnectionString) { Name = connectionName, StoreConnectionNameOnly = true };

            if(parameters is JsonSourceConnectionParameters jsonParams)
                return new JsonDataConnection(jsonParams.JsonSource) { Name = connectionName, StoreConnectionNameOnly = true };

            return null;
        }
    }
}
