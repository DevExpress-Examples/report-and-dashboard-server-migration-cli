using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;
using DevExpress.XtraReports.Web.Extensions;
using AspNetCoreReportDashboardSampleApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDevExpressControls();
builder.Services.AddSingleton<CustomConnectionProvider>();
builder.Services.AddSingleton<IConnectionProviderService>(sp => sp.GetRequiredService<CustomConnectionProvider>());
builder.Services.AddSingleton<IDataSourceWizardConnectionStringsProvider>(sp => sp.GetRequiredService<CustomConnectionProvider>());
builder.Services.AddSingleton<IDataSourceWizardJsonConnectionStorage>(sp => sp.GetRequiredService<CustomConnectionProvider>());
builder.Services.AddSingleton<IJsonDataConnectionProviderService>(sp => sp.GetRequiredService<CustomConnectionProvider>());
builder.Services.AddSingleton<ReportStorageWebExtension, CustomReportStorageWebExtension>();
builder.Services.ConfigureReportingServices(configurator => {
    configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
        viewerConfigurator.UseCachedReportSourceBuilder();
        viewerConfigurator.RegisterConnectionProviderFactory<CustomConnectionProviderFactory>();
        viewerConfigurator.RegisterJsonDataConnectionProviderFactory<CustomJsonConnectionProviderFactory>();
    });
    configurator.ConfigureReportDesigner(designerConfigurator => {
        designerConfigurator.RegisterDataSourceWizardConnectionStringsProvider<CustomConnectionProvider>();
        designerConfigurator.RegisterDataSourceWizardJsonConnectionStorage<CustomConnectionProvider>(true);
    });
});

string dashboardsPath = Path.Combine(builder.Environment.ContentRootPath, "Dashboards");
Directory.CreateDirectory(dashboardsPath);

builder.Services.AddScoped<DashboardConfigurator>(serviceProvider => {
    var configurator = new DashboardConfigurator();
    configurator.SetDashboardStorage(new DashboardFileStorage(dashboardsPath));
    configurator.SetDataSourceStorage(new DataSourceInMemoryStorage());
    configurator.SetConnectionStringsProvider(
        serviceProvider.GetRequiredService<CustomConnectionProvider>());
    return configurator;
});

var app = builder.Build();

if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseDevExpressControls();
app.UseAuthorization();
app.MapStaticAssets();
app.MapDashboardRoute("dashboardControl", "DefaultDashboard");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
