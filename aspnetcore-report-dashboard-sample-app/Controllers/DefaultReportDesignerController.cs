using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;

namespace AspNetCoreReportDashboardSampleApp.Controllers {
    public class DefaultReportDesignerController : ReportDesignerController {
        public DefaultReportDesignerController(IReportDesignerMvcControllerService controllerService)
            : base(controllerService) {
        }
    }
}
