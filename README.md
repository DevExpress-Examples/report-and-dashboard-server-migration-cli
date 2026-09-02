# Data Export CLI Tool for DevExpress Report & Dashboard Server

This repository contains a migration utility and a sample ASP.NET Core application designed to move content from the [DevExpress Report and Dashboard Server](https://docs.devexpress.com/ReportServer/12432/report-and-dashboard-server) to a standalone application built with DevExpress Reports and DevExpress BI Dashboard components.

The tool exports DevExpress Report and Dashboard Server content to local files. It can also automatically copy exported files into the sample application. A developer can then run the sample application and view and edit exported reports and dashboards without a running Report and Dashboard Server instance.

## Repository Structure

The repository includes two tools that are intended to be used together:

* [Data Export CLI Tool](#data-export-cli-tool) - exports content from DevExpress Report and Dashboard Server to local files.
* [ASP.NET Core Report and Dashboard Sample App](#aspnet-core-report-and-dashboard-sample-app) - loads exported files so you can view and edit reports and dashboards in a standalone web application.

## Data Export CLI Tool

[rds-data-export-cli-tool](./rds-data-export-cli-tool) is a command-line tool that connects to a DevExpress Report and Dashboard Server instance and exports the following content:

* Report layouts
* Dashboard layouts
* Data model definitions, including connection settings and schema metadata
* Users, groups, and permissions

Exported data is stored in the following files: JSON metadata files, report layouts, and dashboard layouts.

The tool communicates with DevExpress Report and Dashboard Server through its HTTP API. During the export process, it authenticates with the server, retrieves all supported content, and writes it to the specified output directory.

The tool can automatically copy exported reports, dashboards, and data models into the accompanying `aspnetcore-report-dashboard-sample-app` application, allowing you to run the sample immediately after the export operation completes.

### Build the Solution

Open the solution in Visual Studio and build it.

Make certain that both projects build successfully:

* rds-data-export-cli-tool
* aspnetcore-report-dashboard-sample-app

### Prerequisites 

Before you run the export tool, make sure that:

- The DevExpress Report and Dashboard Server HTTP API is available over HTTPS.
- You have a Report and Dabhoard Server user account with permission to read all reports, dashboards, data models, users, groups, and permission assignments that you want to export.
- The server certificate is trusted by the machine that runs the export tool.

The export tool does not automatically authenticate with the Windows account of the current process. When Report and Dashboard Server uses Windows Authentication, supply credentials for a corresponding Report and Dashboard Server account that has sufficient permissions.

If the server uses a self-signed or otherwise untrusted certificate, you can use the --insecure option. This disables TLS certificate validation and should only be used on trusted networks.

### Run the Export Tool

Run `rds-data-export-cli-tool.exe` and specify the appropriate Report and Dashboard Server connection settings. 

Example:

`rds-data-export-cli-tool.exe --url http://localhost:83 --username Admin --integrate`

If you omit `--password`, the tool reads the password from the `RDS_USER_PASSWORD` environment variable. If neither option is specified, the tool prompts the password interactively with masked input. Avoid passing `--password` on the command line, because it is visible in process listings, shell history, and audit logs.

Required Arguments:

| Argument                | Description                        |
| ----------------------- | ---------------------------------- |
| `--url <serverUrl>`     | DevExpress Report and Dashboard Server base URL.  |
| `--username <userName>` | DevExpress Report and Dashboard Server user name. |

Optional Arguments:

| Argument                | Description                                                     |
| ----------------------- | --------------------------------------------------------------- |
| `--password <password>` | DevExpress Report and Dashboard Server password. If omitted, it is read from the `RDS_USER_PASSWORD` environment variable or prompted for interactively with masked input.|
| `--export-path <path>`  | Export output directory.                                         |
| `--by-category`         | Organize exported files into category subfolders.                |
| `--integrate`           | Copy exported data to the `aspnetcore-report-dashboard-sample-app` project. |
| `--insecure`            | Skip TLS certificate validation. Use this option when the server uses an HTTPS URL with a self-signed or otherwise untrusted certificate. The connection is not protected against interception, so use it only on trusted networks. |
| `--help`, `-h`          | Show help information.                                           |

### Export Report and Dashboard Server Content

The tool connects to the specified DevExpress Report and Dashboard Server instance and exports:

* Reports
* Dashboards
* Data models
* User accounts
* User groups
* Permissions

By default, exported files are saved to the Export directory of the `rds-data-export-cli-tool` project.

If you use the `--integrate` option, the tool also copies the exported reports, dashboards, and data model files into the root directory of the `aspnetcore-report-dashboard-sample-app` project. The sample application reads report layouts, dashboard layouts, and data model metadata from these directories at runtime.

If you do not use --integrate, copy these directories manually:

* Export/reports -> aspnetcore-report-dashboard-sample-app/reports

* Export/dashboards -> aspnetcore-report-dashboard-sample-app/dashboards

* Export/data-models -> aspnetcore-report-dashboard-sample-app/data-models

#### Output Structure

The export tool creates the following file structure:

```
<export directory>/
├── documents.json
├── reports/
│   └── ReportName.repx
├── dashboards/
│   └── DashboardName.json
├── data-models/
│   ├── data-models.json
│   └── ModelName_schema.json
└── users-and-roles/
    ├── users.json
    └── usergroups.json
```

When the `--by-category` option is used, reports and dashboards are organized into category subfolders.

> [!WARNING]
> **The export directory contains sensitive data.** `data-models/data-models.json` includes data connection information, which may contain database credentials, and `users-and-roles/` contains user accounts and permission assignments. Store the export output in a secure, access-controlled location, exclude it from source control, and delete it when it is no longer needed.

### Limitations

* The tool exports only the latest revision of each report and dashboard layout. Previous revisions and revision history are not exported.

* The Scheduler module is not exported.

* When DevExpress Report and Dashboard Server is configured to use Windows Authentication, a corresponding user account must exist and have sufficient permissions. Otherwise, `rds-data-export-cli-tool` cannot extract the required data.

## ASP.NET Core Report and Dashboard Sample App

[aspnetcore-report-dashboard-sample-app](./aspnetcore-report-dashboard-sample-app) is an ASP.NET Core application that demonstrates how to work with content exported from DevExpress Report and Dashboard Server.

The application includes the following DevExpress components:

* DevExpress Report Viewer
* DevExpress Report Designer
* DevExpress Dashboard

The sample application does not connect to DevExpress Report and Dashboard Server at runtime. It loads exported reports, dashboards, and data model metadata from the exported files.

The application uses custom report storage to load and save report layouts, dashboard file storage to load dashboard layouts, and a custom connection provider to reconstruct data connections from the exported data model metadata.

This architecture allows you to view, edit, and explore exported reports and dashboards in a standalone ASP.NET Core application. Use this architecture as a reference implementation for integrating exported Report and Dashboard Server content into your own applications.

This repository assumes you clone the source code and run `aspnetcore-report-dashboard-sample-app` directly from the cloned repository.

The sample app relies on a known folder structure and relative paths inside the repository:

* `aspnetcore-report-dashboard-sample-app/reports`
* `aspnetcore-report-dashboard-sample-app/dashboards`
* `aspnetcore-report-dashboard-sample-app/data-models`

When you run the export tool with `--integrate`, these folders are populated automatically in that project location. If you run the sample app from a different location, rename folders, or change the expected structure, update the corresponding storage and connection configuration in the sample application.

### Run the Sample Application

After the export operation is complete, run `aspnetcore-report-dashboard-sample-app`.

You can run it from Visual Studio or use the command line:

`dotnet run --project aspnetcore-report-dashboard-sample-app`

### View Exported Content

Open the sample "DevExpress Reports & Dashboards" application in a browser and use the available pages:

* DevExpress Report Viewer - view exported reports.
* DevExpress Report Designer - open, edit, and save exported report layouts.
* DevExpress BI Dashboard - explore exported dashboards.

## Extend This Sample Application

You can use this sample (`aspnetcore-report-dashboard-sample-app`) as a starting point for a production application.

### Add Access Control and Data Protection

Apply a security-first baseline before moving the sample to production:

- Integrate authentication and authorization. Enforce role-based access to report, dashboard, and designer operations.
- Configure dashboard backend authorization and restrict dashboard designer access to authorized users. For additional information, see [Web Dashboard Security Considerations](https://docs.devexpress.com/Dashboard/118651/web-dashboard/integrate-dashboard-component/aspnet-core-dashboard-control/security-considerations-in-asp-net-core).
- Use DevExpress storage extension points to move away from file system storage for reports and dashboards. Consider database or cloud storage to improve security, centralize access control, and simplify backup and recovery.
- Protect secrets and connection metadata by using secure configuration and secret management mechanisms.
- Restrict access to dashboard data connections. Avoid exposing sensitive connection information to clients.
- Use HTTPS and trusted certificates in all environments.

You can also refer to the [ASP.NET Core Reporting Best Practices](https://docs.devexpress.com/XtraReports/402190/web-reporting/asp-net-core-reporting/asp-net-core-reporting-best-practices) code example to see how some of these security practices can be implemented in a production application.

Refer to the following help topics for additional information:

- [Report Storage](https://docs.devexpress.com/XtraReports/400211/web-reporting/asp-net-core-reporting/end-user-report-designer-in-asp-net-applications/add-a-report-storage)
- [Dashboard Storage](https://docs.devexpress.com/Dashboard/16979/web-dashboard/integrate-dashboard-component/dashboard-backend/prepare-dashboard-storage)
- [Dashboard Security](https://docs.devexpress.com/Dashboard/118651/web-dashboard/integrate-dashboard-component/aspnet-core-dashboard-control/security-considerations-in-asp-net-core)
- [Report Security Best Practices](https://docs.devexpress.com/XtraReports/402190/web-reporting/asp-net-core-reporting/asp-net-core-reporting-best-practices)

### Add Monitoring and Diagnostics

Add structured logging, health monitoring, and production-grade error handling to your app.

If you deploy an application to multiple servers or containers, configure centralized logging, distributed caching, and shared storage for report documents, dashboard layouts, document cache, and other persistent application data.

Refer to the following help topics for additional information:

- [Report Diagnostics](https://docs.devexpress.com/XtraReports/401687/web-reporting/troubleshooting/application-diagnostics)
- [Dashboard Diagnostics](https://docs.devexpress.com/Dashboard/400026/web-dashboard/integrate-dashboard-component/aspnet-core-dashboard-control/handle-and-log-server-side-errors-in-asp-net-core)
- [Report Troubleshooting](https://docs.devexpress.com/XtraReports/401726/web-reporting/troubleshooting)
- [Dashboard Troubleshooting](https://docs.devexpress.com/Dashboard/403866/basic-concepts-and-terminology/bi-dashboard-performance/performance-troubleshooting)

### Add Localization Support

If necessary, localize reporting and dashboard UI elements and related resources.

Refer to the following help topics for additional information:

- [Report Localization](https://docs.devexpress.com/XtraReports/400932/web-reporting/common-features/localization/localization-in-asp-net-core-reporting-applications)
- [Dashboard Localization](https://docs.devexpress.com/Dashboard/402535/web-dashboard/integrate-dashboard-component/aspnet-core-dashboard-control/localization)

### Apply Themes and Customize Styles

Apply consistent visual styling to both Reporting and Dashboard components so they match your application design.

Refer to the following help topics for additional information:

- [Report Themes and Styles](https://docs.devexpress.com/XtraReports/403927/web-reporting/common-features/themes-and-styles)
- [Dashboard Themes and Styles](https://docs.devexpress.com/Dashboard/119993/web-dashboard/integrate-dashboard-component/aspnet-core-dashboard-control/themes-and-styles )

### Activate Web Accessibility Features

Activate accessibility features for reporting pages and dashboard interactions.

Refer to the following help topics for additional information:

- [Report Accessibility](https://docs.devexpress.com/XtraReports/402561/web-reporting/common-features/web-accessibility)
- [Dashboard Accessibility (Viewer Mode)](https://docs.devexpress.com/Dashboard/404964/web-dashboard/dashboard-viewer-web-accessibility)

### Rework Custom Connection Providers

The sample application already includes a custom connection provider with support for `MSSqlServer`, `UriJsonSource`, and `CustomJsonSource`.

You can extend this implementation for the following purposes:

- Support additional report and dashboard data source types.
- Resolve tenant-specific or user-specific connections.
- Integrate cloud-hosted databases and secret providers.
- Customize Dashboard/Report Data Source Wizard behavior.
- Restrict available data sources for specific users or roles.

Refer to the following help topics for additional information:

- [Report Data Sources and Connections](https://docs.devexpress.com/XtraReports/401896/web-reporting/asp-net-core-reporting/end-user-report-designer-in-asp-net-applications/use-data-sources-and-connections)
- [Dashboard Data Sources](https://docs.devexpress.com/Dashboard/116980/web-dashboard/create-dashboards-on-the-web/provide-data)

### Add Support for User-Specific Functions

[User-Specific Functions](http://docs.devexpress.com/ReportServer/400990/user-specific-functions) configured in DevExpress Report and Dashboard Server are not migrated automatically. If an exported layout uses these functions, implement and register equivalent custom functions in the standalone application.

Refer to the following help topics for additional information:

- [Dashboard Custom Functions](https://docs.devexpress.com/Dashboard/403419/common-features/advanced-analytics/aggregations/custom-aggregate-functions)
- [Custom Functions](https://docs.devexpress.com/XtraReports/403888/feature-guide-to-devexpress-reports/use-expressions/custom-functions)

### Add Scheduler Replacement

This sample application does not include a replacement for the DevExpress Report and Dashboard Server scheduler. Add an application-specific scheduling mechanism if you need automated report or dashboard jobs.

Depending on your application architecture and hosting environment, you can implement scheduled execution by using a background job framework or platform-specific scheduling services. For example, Hangfire, Quartz.NET, Azure Functions, or other scheduling solutions.

Because report generation is CPU and memory intensive, consider executing scheduled jobs outside the main application process to improve scalability and reliability.

Reports can be generated and exported programmatically without any UI. For additional information, see:

- [Print and Export Reports without a Preview](https://docs.devexpress.com/XtraReports/14950/feature-guide-to-devexpress-reports/store-and-distribute-reports/print-and-export-reports-without-a-preview)

### Optimize Memory Usage and Performance

Profile and tune memory usage and performance for report generation, document processing, and dashboard data loading in production workloads.

Refer to the following help topics for additional information:

- [Memory Usage in Reporting - Best Practices](https://docs.devexpress.com/XtraReports/405035/common-information/reporting-memory-usage)
- [BI Dashboard Performance](https://docs.devexpress.com/Dashboard/403860/basic-concepts-and-terminology/bi-dashboard-performance)

### Add Web Farm and Web Garden Support

If you scale out to multiple worker processes or servers, configure Reporting for distributed execution.

Dashboard components do not require additional configuration for this scenario. The following guidance is specific to Reporting because of its document generation architecture.

Refer to the following help topic for additional information:

- [Web Farm and Web Garden Support](https://docs.devexpress.com/XtraReports/5199/web-reporting/common-features/web-farms-and-web-gardens-support)

### Prepare for Production Deployment

This sample application can be deployed to Docker, Kubernetes, Microsoft Azure, AWS, Google Cloud, and other cloud platforms. It can also run on any hosting environment that supports ASP.NET Core.

Refer to the following help topic for additional information:

- [Cloud Integration](https://docs.devexpress.com/XtraReports/404819/cloud-integration)

### Configure Your App for Linux

If you deploy outside Windows, configure runtime dependencies, fonts, persistent storage, distributed caches, secure secret management, and other platform-specific settings accordingly.

The available guidance focuses on Reporting, but the same runtime considerations generally apply to Dashboard applications.

Refer to the following help topics for additional information:

- [Use Reporting on Linux](https://docs.devexpress.com/XtraReports/404221/common-information/dot-net-and-net-core-support/use-reporting-on-linux)
- [DevExpress Drawing Graphics Library](https://docs.devexpress.com/CoreLibraries/404247/devexpress-drawing-library)

## Useful Resources
    
* [DevExpress Report and Dashboard Server documentation](https://docs.devexpress.com/ReportServer/12432/report-and-dashboard-server)
* [DevExpress Reports documentation](https://docs.devexpress.com/XtraReports/2162/reporting)
* [DevExpress BI Dashboard documentation](https://docs.devexpress.com/Dashboard/12049/dashboard)
   
