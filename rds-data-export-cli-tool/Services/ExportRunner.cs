using RdsDataExportCliTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal class ExportRunner {
        private readonly IReportServerClient _client;
        private readonly IDocumentExporter _documentExporter;
        private readonly IDataModelExporter _dataModelExporter;
        private readonly IUserExporter _userExporter;
        private readonly IConsoleService _console;
        private readonly ExportOptions _options;

        public ExportRunner(
            IReportServerClient client,
            IDocumentExporter documentExporter,
            IDataModelExporter dataModelExporter,
            IUserExporter userExporter,
            IConsoleService console,
            ExportOptions options) {
            _client = client;
            _documentExporter = documentExporter;
            _dataModelExporter = dataModelExporter;
            _userExporter = userExporter;
            _console = console;
            _options = options;
        }

        public async Task<bool> RunAsync() {
            try {
                bool isAuthenticated = await _client.AuthenticateAsync(_options.Username, _options.Password);
                if(!isAuthenticated)
                    return false;

                string outputDir = string.IsNullOrEmpty(_options.ExportPath)
                    ? Path.Combine(Directory.GetCurrentDirectory(), "Export")
                    : Path.GetFullPath(_options.ExportPath);

                Directory.CreateDirectory(outputDir);
                FSHelper.ClearExportArtifacts(outputDir, _console);

                // Build category lookup
                var categories = await _client.GetCategoriesAsync();
                var categoryMap = new Dictionary<int, string>();
                foreach(var cat in categories)
                    categoryMap[cat.Id] = cat.Name;

                // Export documents (reports & dashboards)
                await _documentExporter.ExportRDSDataAsync(outputDir, _options.ExportByCategory, categoryMap);

                // Export data models
                await _dataModelExporter.ExportDataModelsAsync(outputDir);

                // Export users, roles, and permissions
                await _userExporter.ExportUsersAndRolesAsync(outputDir);

                _console.WriteLine();
                _console.WriteLine("=== Export complete ===");
                _console.WriteLine($"Output directory: {outputDir}");
                _console.WriteLine();

                if(_options.IntegrateIntoTheExample) {
                    string projectTargetDir = GetExampleProjectTargetPath();
                    _console.WriteLine();
                    _console.WriteLine($"Integrating exported documents into MVC Example App: {projectTargetDir}");

                    CopyExportedFolder(outputDir, "reports", projectTargetDir, "Reports");
                    CopyExportedFolder(outputDir, "dashboards", projectTargetDir, "Dashboards");
                    CopyExportedFolder(outputDir, "data-models", projectTargetDir, "DataModels");

                    _console.WriteLine("Integration complete.");
                }

                return true;
            } catch(Exception ex) {
                _console.WriteLine($"Unexpected error during export: {ExceptionHelper.Unwrap(ex).Message}");
                return false;
            }
        }

        private string GetExampleProjectTargetPath() {
            const string exampleProjectFolder = "aspnetcore-report-dashboard-sample-app";
            string currentDir = Directory.GetCurrentDirectory();
            while(!string.IsNullOrEmpty(currentDir)) {
                string targetDir = Path.Combine(currentDir, exampleProjectFolder);
                if(Directory.Exists(targetDir)) {
                    return targetDir;
                }
                currentDir = Path.GetDirectoryName(currentDir);
            }
            throw new DirectoryNotFoundException($"Could not find the '{exampleProjectFolder}' directory.");
        }

        private void CopyExportedFolder(string outputDir, string srcSubFolder, string mvcTargetDir, string dstSubFolder) {
            string src = Path.Combine(outputDir, srcSubFolder);
            string dst = Path.Combine(mvcTargetDir, dstSubFolder);
            if(!Directory.Exists(src))
                return;
            FSHelper.ClearDirectory(dst, _console);
            FSHelper.CopyDirectory(src, dst);
        }
    }
}
