using JDMS.Application.Constants;
using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class InvoicesController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IUnitOfWork _unitOfWork;

    public InvoicesController(IInvoiceService invoiceService, IUnitOfWork unitOfWork)
    {
        _invoiceService = invoiceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Invoices.Query()
            .Include(i => i.Order).ThenInclude(o => o.Customer)
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
        return View(list);
    }

    public async Task<IActionResult> Print(int orderId, CancellationToken cancellationToken)
    {
        var pdf = await _invoiceService.GeneratePdfAsync(orderId, thermal: false, cancellationToken);
        return File(pdf, "application/pdf", $"invoice-{orderId}.pdf");
    }

    public async Task<IActionResult> PrintThermal(int orderId, CancellationToken cancellationToken)
    {
        var pdf = await _invoiceService.GeneratePdfAsync(orderId, thermal: true, cancellationToken);
        return File(pdf, "application/pdf", $"receipt-{orderId}.pdf");
    }

    public async Task<IActionResult> Download(int orderId, CancellationToken cancellationToken)
        => await Print(orderId, cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Generate(int orderId, CancellationToken cancellationToken)
    {
        await _invoiceService.CreateInvoiceForOrderAsync(orderId, cancellationToken);
        return RedirectToAction(nameof(Print), new { orderId });
    }
}
