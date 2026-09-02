using RdsDataExportCliTool.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal class ReportServerClient : IReportServerClient, IDisposable {
        private readonly HttpClient _httpClient;
        private readonly IConsoleService _console;
        private bool _disposed;

        public ReportServerClient(string serverUrl, IConsoleService console) {
            _console = console;
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            _httpClient = new HttpClient(handler) {
                BaseAddress = new Uri(serverUrl),
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<bool> AuthenticateAsync(string username, string password) {
            var tokenContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var tokenResponse = await _httpClient.PostAsync("/oauth/token", tokenContent);
            if(!tokenResponse.IsSuccessStatusCode) {
                _console.WriteLine($"Login failed: {tokenResponse.StatusCode}");
                var errorBody = await tokenResponse.Content.ReadAsStringAsync();
                if(!string.IsNullOrEmpty(errorBody))
                    _console.WriteLine("  Response: " + errorBody);
                return false;
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);

            if(tokenData == null || !tokenData.TryGetValue("access_token", out string accessToken)
                || string.IsNullOrEmpty(accessToken)) {
                _console.WriteLine("Login failed: server response did not contain a valid access token.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            _console.WriteLine("Login successful.");
            return true;
        }

        public async Task<List<CategoryModel>> GetCategoriesAsync() {
            var response = await _httpClient.GetStringAsync("/api/categories");
            return JsonConvert.DeserializeObject<List<CategoryModel>>(response) ?? new List<CategoryModel>();
        }

        public async Task<List<DocumentModel>> GetDocumentsAsync() {
            var response = await _httpClient.GetStringAsync("/api/documents");
            return JsonConvert.DeserializeObject<List<DocumentModel>>(response) ?? new List<DocumentModel>();
        }

        public async Task<HttpResponseMessage> ExportDocumentLayoutAsync(int documentId) {
            return await _httpClient.GetAsync($"/api/documentdesign/{documentId}/exportlayout");
        }

        public async Task<List<DataModelListModel>> GetDataModelsAsync() {
            var response = await _httpClient.GetStringAsync("/api/datamodels");
            return JsonConvert.DeserializeObject<List<DataModelListModel>>(response) ?? new List<DataModelListModel>();
        }

        public async Task<string> GetDataModelConnectionInfoAsync(int dataModelId) {
            return await _httpClient.GetStringAsync($"/api/datamodels/{dataModelId}/connection");
        }

        public async Task<string> GetDataModelSchemaAsync(int dataModelId) {
            return await _httpClient.GetStringAsync($"/api/datamodels/{dataModelId}/tables");
        }

        public async Task<string> GetUsersAsync() {
            return await _httpClient.GetStringAsync("/api/users");
        }

        public async Task<string> GetUserPermissionsAsync(int userId) {
            return await _httpClient.GetStringAsync($"/api/users/{userId}/permissions");
        }

        public async Task<string> GetUserGroupsAsync() {
            return await _httpClient.GetStringAsync("/api/usergroups");
        }

        public async Task<string> GetUserGroupPermissionsAsync(int groupId) {
            return await _httpClient.GetStringAsync($"/api/usergroups/{groupId}/permissions");
        }

        protected virtual void Dispose(bool disposing) {
            if(!_disposed) {
                if(disposing)
                    _httpClient?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
    }
}
