using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal class DataModelExporter : IDataModelExporter {
        private readonly IReportServerClient _client;
        private readonly IConsoleService _console;

        public DataModelExporter(IReportServerClient client, IConsoleService console) {
            _client = client;
            _console = console;
        }

        public async Task ExportDataModelsAsync(string outputDir) {
            _console.WriteLine();
            _console.WriteLine("Exporting data models...");
            try {
                var dataModels = await _client.GetDataModelsAsync();
                _console.WriteLine($"Found {dataModels.Count} data models.");

                string dataModelsDir = Path.Combine(outputDir, "data-models");
                Directory.CreateDirectory(dataModelsDir);

                var enrichedModels = new JArray();
                foreach(var dm in dataModels) {
                    var modelObj = JObject.FromObject(dm);
                    try {
                        var connectionResponse = await _client.GetDataModelConnectionInfoAsync(dm.Id);
                        var connectionDetail = JObject.Parse(connectionResponse);
                        var providerModel = connectionDetail["providerModel"];
                        if(providerModel != null) {
                            modelObj["ConnectionInfo"] = providerModel;
                        }
                    } catch(Exception ex) {
                        _console.WriteLine($"  [WARN] Could not get connection info for '{dm.Name}': {ExceptionHelper.Unwrap(ex).Message}");
                    }
                    enrichedModels.Add(modelObj);
                }

                File.WriteAllText(
                    Path.Combine(dataModelsDir, "data-models.json"),
                    enrichedModels.ToString(Formatting.Indented));

                foreach(var dm in dataModels) {
                    try {
                        var schemaResponse = await _client.GetDataModelSchemaAsync(dm.Id);
                        string safeFileName = FSHelper.SanitizeFileName(dm.Name) + "_schema.json";
                        File.WriteAllText(
                            Path.Combine(dataModelsDir, safeFileName),
                            JToken.Parse(schemaResponse).ToString(Formatting.Indented));
                        _console.WriteLine($"  [OK] Data model schema: {dm.Name}");
                    } catch(Exception ex) {
                        _console.WriteLine($"  [WARN] Could not export schema for '{dm.Name}': {ExceptionHelper.Unwrap(ex).Message}");
                    }
                }
            } catch(Exception ex) {
                _console.WriteLine($"  [WARN] Could not export data models: {ExceptionHelper.Unwrap(ex).Message}");
            }
        }
    }
}
