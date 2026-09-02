using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;

namespace AspNetCoreReportDashboardSampleApp.Controllers {
    public class DefaultWebDocumentViewerController : WebDocumentViewerController {
        public DefaultWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService)
            : base(controllerService) {
        }
    }
}
