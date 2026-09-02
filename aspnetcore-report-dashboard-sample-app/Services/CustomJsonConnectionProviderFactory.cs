using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Web;

namespace AspNetCoreReportDashboardSampleApp.Services {
    public class CustomJsonConnectionProviderFactory : IJsonDataConnectionProviderFactory {
        private readonly IJsonDataConnectionProviderService _connectionProvider;

        public CustomJsonConnectionProviderFactory(IJsonDataConnectionProviderService connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public IJsonDataConnectionProviderService Create() {
            return _connectionProvider;
        }
    }
}
