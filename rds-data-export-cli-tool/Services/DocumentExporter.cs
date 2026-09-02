using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal class DocumentExporter : IDocumentExporter {
        private readonly IReportServerClient _client;
        private readonly IConsoleService _console;

        public DocumentExporter(IReportServerClient client, IConsoleService console) {
            _client = client;
            _console = console;
        }

        public async Task ExportRDSDataAsync(string outputDir, bool exportByCategory, Dictionary<int, string> categoryMap) {
            var documents = await _client.GetDocumentsAsync();
            _console.WriteLine($"Found {documents.Count} documents.");

            File.WriteAllText(
                Path.Combine(outputDir, "documents.json"),
                JsonConvert.SerializeObject(documents, Formatting.Indented));

            int succeeded = 0;
            int failed = 0;

            for(int i = 0; i < documents.Count; i++) {
                var doc = documents[i];
                _console.Write($"  [{i + 1}/{documents.Count}] Exporting '{doc.Name}'... ");

                string folderName = doc.DocumentType == "Dashboard" ? "dashboards" : "reports";
                string targetDir;

                if(exportByCategory) {
                    if(!categoryMap.TryGetValue(doc.CategoryId, out string categoryName))
                        categoryName = "Uncategorized";
                    categoryName = FSHelper.SanitizePathSegment(categoryName);
                    targetDir = Path.Combine(outputDir, folderName, categoryName);
                } else {
                    targetDir = Path.Combine(outputDir, folderName);
                }

                Directory.CreateDirectory(targetDir);

                var exportResponse = await _client.ExportDocumentLayoutAsync(doc.Id);
                if(!exportResponse.IsSuccessStatusCode) {
                    _console.WriteLine($"FAIL ({exportResponse.StatusCode})");
                    failed++;
                    continue;
                }

                string fileName = doc.Name;
                if(exportResponse.Content.Headers.ContentDisposition != null &&
                    !string.IsNullOrEmpty(exportResponse.Content.Headers.ContentDisposition.FileName)) {
                    fileName = exportResponse.Content.Headers.ContentDisposition.FileName.Trim('"');
                }

                fileName = FSHelper.SanitizeFileName(fileName);
                string filePath = Path.Combine(targetDir, fileName);

                if(File.Exists(filePath)) {
                    string baseName = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    fileName = $"{baseName}_{doc.Id}{ext}";
                    filePath = Path.Combine(targetDir, fileName);
                }

                var content = await exportResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(filePath, content);

                _console.WriteLine("OK");
                succeeded++;
            }

            _console.WriteLine($"Documents: {succeeded} exported, {failed} failed.");
        }
    }
}
