using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal interface IDataModelExporter {
        Task ExportDataModelsAsync(string outputDir);
    }
}
