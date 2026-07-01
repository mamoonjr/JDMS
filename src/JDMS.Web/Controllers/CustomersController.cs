using ClosedXML.Excel;
using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class CustomersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CustomersController(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Customers.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FullName.Contains(search) || c.MobileNumber.Contains(search) || c.CustomerCode.Contains(search));

        var list = await query.OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                FullName = c.FullName,
                MobileNumber = c.MobileNumber,
                SecondaryMobile = c.SecondaryMobile,
                Email = c.Email,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            }).ToListAsync(cancellationToken);

        ViewBag.Search = search;
        return View(list);
    }

    public IActionResult Create() => View(new CustomerCreateDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerCreateDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var count = await _unitOfWork.Customers.Query().CountAsync(cancellationToken);
        var entity = new Customer
        {
            CustomerCode = $"CUS-{(count + 1):D6}",
            FullName = model.FullName,
            MobileNumber = model.MobileNumber,
            SecondaryMobile = model.SecondaryMobile,
            Email = model.Email,
            Notes = model.Notes
        };
        await _unitOfWork.Customers.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(AuditActionType.CustomerCreated, "Customer", entity.Id.ToString(), cancellationToken: cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var c = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (c == null) return NotFound();
        return View(new CustomerCreateDto
        {
            FullName = c.FullName,
            MobileNumber = c.MobileNumber,
            SecondaryMobile = c.SecondaryMobile,
            Email = c.Email,
            Notes = c.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerCreateDto model, CancellationToken cancellationToken)
    {
        var c = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (c == null) return NotFound();
        c.FullName = model.FullName;
        c.MobileNumber = model.MobileNumber;
        c.SecondaryMobile = model.SecondaryMobile;
        c.Email = model.Email;
        c.Notes = model.Notes;
        await _unitOfWork.Customers.UpdateAsync(c, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(AuditActionType.CustomerUpdated, "Customer", id.ToString(), cancellationToken: cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var c = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (c != null)
        {
            await _unitOfWork.Customers.DeleteAsync(c, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditService.LogAsync(AuditActionType.CustomerDeleted, "Customer", id.ToString(), cancellationToken: cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var rows = await _unitOfWork.Customers.Query().AsNoTracking().ToListAsync(cancellationToken);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("العملاء");
        ws.Cell(1, 1).Value = "الرمز";
        ws.Cell(1, 2).Value = "الاسم";
        ws.Cell(1, 3).Value = "الجوال";
        ws.Cell(1, 4).Value = "البريد";
        for (var i = 0; i < rows.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = rows[i].CustomerCode;
            ws.Cell(i + 2, 2).Value = rows[i].FullName;
            ws.Cell(i + 2, 3).Value = rows[i].MobileNumber;
            ws.Cell(i + 2, 4).Value = rows[i].Email;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "customers.xlsx");
    }
}
