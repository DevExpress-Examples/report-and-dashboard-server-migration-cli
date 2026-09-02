using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;

namespace AspNetCoreReportDashboardSampleApp.Services {
    public class CustomReportStorageWebExtension : ReportStorageWebExtension {
        private readonly string _reportDirectory;

        public CustomReportStorageWebExtension(IWebHostEnvironment env) {
            _reportDirectory = Path.Combine(env.ContentRootPath, "Reports");
            if(!Directory.Exists(_reportDirectory))
                Directory.CreateDirectory(_reportDirectory);
        }

        public override bool IsValidUrl(string url) {
            if(string.IsNullOrWhiteSpace(url))
                return false;
            if(url.Contains(".."))
                return false;
            if(url.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;
            return true;
        }

        public override bool CanSetData(string url) => IsValidUrl(url);

        public override byte[] GetData(string url) {
            try {
                var files = Directory.GetFiles(_reportDirectory, "*.repx", SearchOption.AllDirectories);
                var filePath = files.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f) == url || Path.GetFileName(f) == url);

                if(filePath != null)
                    return File.ReadAllBytes(filePath);
            } catch(Exception ex) {
                throw new DevExpress.XtraReports.Web.ClientControls.FaultException("Could not get report data.", ex);
            }
            throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Could not find report '{url}'.");
        }

        public override Dictionary<string, string> GetUrls() {
            if(!Directory.Exists(_reportDirectory))
                return new Dictionary<string, string>();

            return Directory.GetFiles(_reportDirectory, "*.repx", SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(x => x != null)
                .Distinct()
                .ToDictionary(x => x!, x => x!);
        }

        public override void SetData(XtraReport report, string url) {
            var filePath = Path.Combine(_reportDirectory, url + ".repx");
            report.SaveLayoutToXml(filePath);
        }

        public override string SetNewData(XtraReport report, string defaultUrl) {
            SetData(report, defaultUrl);
            return defaultUrl;
        }
    }
}
