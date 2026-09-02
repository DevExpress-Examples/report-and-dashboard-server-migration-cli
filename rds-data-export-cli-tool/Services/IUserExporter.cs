using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal interface IUserExporter {
        Task ExportUsersAndRolesAsync(string outputDir);
    }
}
