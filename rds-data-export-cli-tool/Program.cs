using RdsDataExportCliTool.Models;
using RdsDataExportCliTool.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RdsDataExportCliTool {
    class Program {
        static async Task<int> Main(string[] args) {
            IConsoleService console = new ConsoleService();

            if(args.Length == 0 || HasFlag(args, "--help") || HasFlag(args, "-h")) {
                PrintUsage(console);
                return 0;
            }

            try {
                string serverUrl = (GetArg(args, "--url") ?? string.Empty).TrimEnd('/');
                if(string.IsNullOrWhiteSpace(serverUrl)) {
                    console.WriteLine("Error: --url is required.");
                    PrintUsage(console);
                    return 1;
                }
                if(!Uri.TryCreate(serverUrl, UriKind.Absolute, out _)) {
                    console.WriteLine("Error: --url must be a valid absolute URL.");
                    return 1;
                }

                string username = (GetArg(args, "--username") ?? string.Empty).Trim();
                if(string.IsNullOrWhiteSpace(username)) {
                    console.WriteLine("Error: --username is required.");
                    PrintUsage(console);
                    return 1;
                }

                string password = ResolvePassword(args, console);
                string exportPath = (GetArg(args, "--export-path") ?? string.Empty).Trim();
                bool exportByCategory = HasFlag(args, "--by-category");
                bool integrateIntoExample = HasFlag(args, "--integrate");

                if(HasFlag(args, "--insecure")) {
                    console.WriteLine("WARNING: --insecure is set - TLS certificate validation is disabled. " +
                        "The connection is not protected against interception; use only on trusted networks.");
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                }

                var options = new ExportOptions {
                    ServerUrl = serverUrl,
                    Username = username,
                    Password = password,
                    ExportPath = exportPath,
                    ExportByCategory = exportByCategory,
                    IntegrateIntoTheExample = integrateIntoExample
                };

                bool success;
                using(var reportServerClient = new ReportServerClient(options.ServerUrl, console)) {
                    IDocumentExporter documentExporter = new DocumentExporter(reportServerClient, console);
                    IDataModelExporter dataModelExporter = new DataModelExporter(reportServerClient, console);
                    IUserExporter userExporter = new UserExporter(reportServerClient, console);

                    var runner = new ExportRunner(reportServerClient, documentExporter, dataModelExporter, userExporter, console, options);
                    success = await runner.RunAsync();
                }

                return success ? 0 : 1;
            } catch(Exception ex) {
                console.WriteLine($"Unexpected error: {ExceptionHelper.Unwrap(ex).Message}");
                return 1;
            }
        }

        private const string PasswordEnvVar = "RDS_USER_PASSWORD";

        private static string ResolvePassword(string[] args, IConsoleService console) {
            string? passwordArg = GetArg(args, "--password");
            if(passwordArg != null)
                return passwordArg;

            string? envPassword = Environment.GetEnvironmentVariable(PasswordEnvVar);
            if(envPassword != null)
                return envPassword;

            return console.ReadPassword("Password: ");
        }

        private static string? GetArg(string[] args, string name) {
            for(int i = 0; i < args.Length; i++) {
                if(args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                    return args[i].Substring(name.Length + 1);

                if(string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }

        private static bool HasFlag(string[] args, string flag) {
            foreach(string arg in args)
                if(string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static void PrintUsage(IConsoleService console) {
            console.WriteLine("rds-data-export-cli-tool - DevExpress Report Server data export tool");
            console.WriteLine();
            console.WriteLine("Usage:");
            console.WriteLine("  rds-data-export-cli-tool.exe --url <serverUrl> --username <user> [OPTIONS]");
            console.WriteLine();
            console.WriteLine("Required arguments:");
            console.WriteLine("  --url <serverUrl>       Report Server base URL (e.g. http://localhost:83)");
            console.WriteLine("  --username <user>       Report Server username");
            console.WriteLine();
            console.WriteLine("Optional arguments:");
            console.WriteLine("  --password <pwd>        Report Server password. If omitted, it is read from the");
            console.WriteLine($"                          {PasswordEnvVar} environment variable or prompted for (masked).");
            console.WriteLine("  --export-path <path>    Output directory (default: ./Export)");
            console.WriteLine("  --by-category           Organize exported files into category sub-folders");
            console.WriteLine("  --integrate             Copy exported data into the aspnetcore-report-dashboard-sample-app project");
            console.WriteLine("  --insecure              Skip TLS certificate validation (self-signed or untrusted");
            console.WriteLine("                          server certificates). Use only on trusted networks.");
            console.WriteLine("  --help, -h              Show this help message");
            console.WriteLine();
            console.WriteLine("Examples:");
            console.WriteLine("  rds-data-export-cli-tool.exe --url http://localhost:83 --username Admin");
            console.WriteLine("  rds-data-export-cli-tool.exe --url http://localhost:83 --username Admin --by-category --integrate");
            console.WriteLine("  rds-data-export-cli-tool.exe --url http://localhost:83 --username Admin --export-path C:\\MyExport");
        }
    }
}
