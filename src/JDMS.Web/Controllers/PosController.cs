using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class PosController : Controller
{
    private readonly IPosService _posService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoiceService _invoiceService;

    public PosController(IPosService posService, IUnitOfWork unitOfWork, IInvoiceService invoiceService)
    {
        _posService = posService;
        _unitOfWork = unitOfWork;
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "نقطة البيع";
        ViewData["PosFullScreen"] = true;

        ViewBag.Governorates = await _unitOfWork.Governorates.Query()
            .Where(g => g.IsActive)
            .OrderBy(g => g.NameAr)
            .Select(g => new { g.Id, g.NameAr })
            .ToListAsync(cancellationToken);

        ViewBag.Drivers = await _unitOfWork.Drivers.Query()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DriverName)
            .Select(d => new { d.Id, d.DriverName })
            .ToListAsync(cancellationToken);

        ViewBag.InitialProducts = await _posService.SearchProductsAsync(null, 24, cancellationToken);
        ViewBag.Stats = await _posService.GetStatsAsync(cancellationToken);

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LookupCustomer(string phone, CancellationToken cancellationToken)
    {
        var result = await _posService.LookupByPhoneAsync(phone, cancellationToken);
        return Json(result ?? new CustomerLookupDto { Found = false });
    }

    [HttpGet]
    public async Task<IActionResult> GetAreas(int governorateId, CancellationToken cancellationToken)
    {
        var areas = await _unitOfWork.Areas.Query()
            .Where(a => a.GovernorateId == governorateId && a.IsActive)
            .OrderBy(a => a.NameAr)
            .Select(a => new PosAreaDto { Id = a.Id, NameAr = a.NameAr, DeliveryFee = a.DeliveryFee })
            .ToListAsync(cancellationToken);
        return Json(areas);
    }

    [HttpGet]
    public async Task<IActionResult> SearchProducts(string? q, CancellationToken cancellationToken)
    {
        var products = await _posService.SearchProductsAsync(q, 40, cancellationToken);
        return Json(products);
    }

    [HttpGet]
    public async Task<IActionResult> Stats(CancellationToken cancellationToken) =>
        Json(await _posService.GetStatsAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> AreaFee(int areaId, CancellationToken cancellationToken)
    {
        var fee = await _posService.GetAreaDeliveryFeeAsync(areaId, cancellationToken);
        return Json(new { deliveryFee = fee ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] PosSubmitModel model, CancellationToken cancellationToken)
    {
        var result = await _posService.SubmitAsync(model, cancellationToken);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> Print(int orderId, bool thermal = false, CancellationToken cancellationToken = default)
    {
        var pdf = await _invoiceService.GeneratePdfAsync(orderId, thermal, cancellationToken);
        return File(pdf, "application/pdf", thermal ? $"receipt-{orderId}.pdf" : $"invoice-{orderId}.pdf");
    }
}
