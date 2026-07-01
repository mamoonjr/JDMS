using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IReportService reportService, IUnitOfWork unitOfWork)
    {
        _reportService = reportService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Governorates = await _unitOfWork.Governorates.Query().Select(g => new { g.Id, g.NameAr }).ToListAsync(cancellationToken);
        ViewBag.Drivers = await _unitOfWork.Drivers.Query().Select(d => new { d.Id, d.DriverName }).ToListAsync(cancellationToken);
        return View(new ReportFilterDto());
    }

    [HttpPost]
    public async Task<IActionResult> ViewReport(ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var data = filter.ReportType switch
        {
            "revenue" => await _reportService.GetRevenueReportAsync(filter, cancellationToken),
            "customers" => await _reportService.GetCustomersReportAsync(filter, cancellationToken),
            "drivers" => await _reportService.GetDriversReportAsync(filter, cancellationToken),
            "delivery" => await _reportService.GetDeliveryReportAsync(filter, cancellationToken),
            _ => await _reportService.GetOrdersReportAsync(filter, cancellationToken)
        };
        ViewBag.Filter = filter;
        return View("ReportResult", data);
    }

    [HttpPost]
    public async Task<IActionResult> ExportExcel(ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportOrdersExcelAsync(filter, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report-{filter.ReportType}.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportPdf(ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportOrdersPdfAsync(filter, cancellationToken);
        return File(bytes, "application/pdf", $"report-{filter.ReportType}.pdf");
    }
}
