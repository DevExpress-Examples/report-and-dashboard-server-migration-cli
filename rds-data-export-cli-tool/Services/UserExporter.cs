using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RdsDataExportCliTool.Services {
    internal class UserExporter : IUserExporter {
        private readonly IReportServerClient _client;
        private readonly IConsoleService _console;

        public UserExporter(IReportServerClient client, IConsoleService console) {
            _client = client;
            _console = console;
        }

        public async Task ExportUsersAndRolesAsync(string outputDir) {
            _console.WriteLine();
            _console.WriteLine("Exporting users, roles, and permissions...");
            string usersDir = Path.Combine(outputDir, "users-and-roles");
            Directory.CreateDirectory(usersDir);

            try {                
                var groupsResponse = await _client.GetUserGroupsAsync();
                var groupsArray = JArray.Parse(groupsResponse);
                _console.WriteLine($"Found {groupsArray.Count} user groups (roles).");

                var enrichedGroups = new JArray();
                foreach(var group in groupsArray) {
                    var groupObj = (JObject)group;                 
                    int groupId = groupObj.GetValue("id", StringComparison.OrdinalIgnoreCase)?.ToObject<int>() ?? 0;
                    string groupName = groupObj.GetValue("name", StringComparison.OrdinalIgnoreCase)?.ToString() ?? "(unknown)";

                    try {
                        var permissionsResponse = await _client.GetUserGroupPermissionsAsync(groupId);
                        groupObj["Permissions"] = JArray.Parse(permissionsResponse);
                    } catch(Exception ex) {
                        _console.WriteLine($"  [WARN] Could not get permissions for user group '{groupName}': {ExceptionHelper.Unwrap(ex).Message}");
                    }
                    enrichedGroups.Add(groupObj);
                }

                File.WriteAllText(
                    Path.Combine(usersDir, "usergroups.json"),
                    enrichedGroups.ToString(Formatting.Indented));
                _console.WriteLine("  [OK] usergroups.json");
                
                var usersResponse = await _client.GetUsersAsync();
                var usersArray = JArray.Parse(usersResponse);
                _console.WriteLine($"Found {usersArray.Count} users.");

                var enrichedUsers = new JArray();
                foreach(var user in usersArray) {
                    var userObj = (JObject)user;                    
                    int userId = userObj.GetValue("id", StringComparison.OrdinalIgnoreCase)?.ToObject<int>() ?? 0;
                    string userName = userObj.GetValue("name", StringComparison.OrdinalIgnoreCase)?.ToString() ?? "(unknown)";

                    try {
                        var permissionsResponse = await _client.GetUserPermissionsAsync(userId);
                        userObj["Permissions"] = JArray.Parse(permissionsResponse);
                    } catch(Exception ex) {
                        _console.WriteLine($"  [WARN] Could not get permissions for user '{userName}': {ExceptionHelper.Unwrap(ex).Message}");
                    }
                    enrichedUsers.Add(userObj);
                }

                File.WriteAllText(
                    Path.Combine(usersDir, "users.json"),
                    enrichedUsers.ToString(Formatting.Indented));
                _console.WriteLine("  [OK] users.json");
            } catch(Exception ex) {
                _console.WriteLine($"  [WARN] Could not export users and roles: {ExceptionHelper.Unwrap(ex).Message}");
            }
        }
    }
}
