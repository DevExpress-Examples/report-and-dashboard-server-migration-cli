using RdsDataExportCliTool.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal interface IReportServerClient {
        Task<bool> AuthenticateAsync(string username, string password);
        Task<List<CategoryModel>> GetCategoriesAsync();
        Task<List<DocumentModel>> GetDocumentsAsync();
        Task<HttpResponseMessage> ExportDocumentLayoutAsync(int documentId);
        Task<List<DataModelListModel>> GetDataModelsAsync();
        Task<string> GetDataModelConnectionInfoAsync(int dataModelId);
        Task<string> GetDataModelSchemaAsync(int dataModelId);

        Task<string> GetUsersAsync();
        Task<string> GetUserPermissionsAsync(int userId);
        Task<string> GetUserGroupsAsync();
        Task<string> GetUserGroupPermissionsAsync(int groupId);
    }
}
