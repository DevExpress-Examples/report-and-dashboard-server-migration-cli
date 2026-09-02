using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;

namespace AspNetCoreReportDashboardSampleApp.Services {
    public class CustomConnectionProviderFactory : IConnectionProviderFactory {
        private readonly IConnectionProviderService _connectionProvider;

        public CustomConnectionProviderFactory(IConnectionProviderService connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public IConnectionProviderService Create() {
            return _connectionProvider;
        }
    }
}
