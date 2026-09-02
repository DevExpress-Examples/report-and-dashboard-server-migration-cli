using System.Collections.Generic;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal interface IDocumentExporter {
        Task ExportRDSDataAsync(string outputDir, bool exportByCategory, Dictionary<int, string> categoryMap);
    }
}
