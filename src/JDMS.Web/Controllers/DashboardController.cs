using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JDMS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Stats = await _dashboardService.GetStatsAsync(cancellationToken);
        ViewBag.Charts = await _dashboardService.GetChartsAsync(cancellationToken);
        ViewBag.RecentOrders = await _dashboardService.GetRecentOrdersAsync(10, cancellationToken);
        return View();
    }
}
