using Microsoft.AspNetCore.Mvc;
using AspNetCoreReportDashboardSampleApp.Models;
using System.Diagnostics;

namespace AspNetCoreReportDashboardSampleApp.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) {
        _logger = logger;
    }

    public IActionResult Index() => View();

    public IActionResult ReportViewer() => View();

    public IActionResult Dashboards() => View();

    public IActionResult ReportDesigner() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("Error page rendered. TraceId: {TraceId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
