using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class ExpensesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpensesController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string? search, CancellationToken cancellationToken)
    {
        var from = dateFrom?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var to = dateTo?.Date ?? DateTime.Today;
        if (to < from) (from, to) = (to, from);

        var toExclusive = to.AddDays(1);

        var q = _unitOfWork.Expenses.Query().AsNoTracking()
            .Where(e => e.StartDate >= from && e.StartDate < toExclusive);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.ItemName.Contains(search) || (e.Notes != null && e.Notes.Contains(search)));

        var expenses = await q
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.Id)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ItemName = e.ItemName,
                Amount = e.Amount,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var stats = await BuildStatsAsync(from, toExclusive, search, cancellationToken);

        return View(new ExpenseIndexViewModel
        {
            Filter = new ExpenseFilterDto { DateFrom = from, DateTo = to, Search = search },
            Expenses = expenses,
            Stats = stats
        });
    }

    public IActionResult Create() => View(new ExpenseCreateDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseCreateDto model, CancellationToken cancellationToken)
    {
        if (model.EndDate.HasValue && model.EndDate.Value.Date < model.StartDate.Date)
            ModelState.AddModelError(nameof(model.EndDate), "تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

        if (!ModelState.IsValid) return View(model);

        await _unitOfWork.Expenses.AddAsync(new Expense
        {
            ItemName = model.ItemName.Trim(),
            Amount = model.Amount,
            StartDate = model.StartDate.Date,
            EndDate = model.EndDate?.Date,
            Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim()
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var e = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (e == null) return NotFound();
        return View(new ExpenseCreateDto
        {
            ItemName = e.ItemName,
            Amount = e.Amount,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Notes = e.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseCreateDto model, CancellationToken cancellationToken)
    {
        if (model.EndDate.HasValue && model.EndDate.Value.Date < model.StartDate.Date)
            ModelState.AddModelError(nameof(model.EndDate), "تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

        if (!ModelState.IsValid) return View(model);

        var e = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (e == null) return NotFound();

        e.ItemName = model.ItemName.Trim();
        e.Amount = model.Amount;
        e.StartDate = model.StartDate.Date;
        e.EndDate = model.EndDate?.Date;
        e.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
        e.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Expenses.UpdateAsync(e, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var e = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (e != null)
        {
            await _unitOfWork.Expenses.DeleteAsync(e, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<ExpenseStatsDto> BuildStatsAsync(DateTime from, DateTime toExclusive, string? search, CancellationToken cancellationToken)
    {
        var q = _unitOfWork.Expenses.Query().AsNoTracking()
            .Where(e => e.StartDate >= from && e.StartDate < toExclusive);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.ItemName.Contains(search) || (e.Notes != null && e.Notes.Contains(search)));

        var total = await q.SumAsync(e => e.Amount, cancellationToken);
        var count = await q.CountAsync(cancellationToken);

        var byItem = await q
            .GroupBy(e => e.ItemName)
            .Select(g => new ExpenseItemSummaryDto
            {
                ItemName = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new ExpenseStatsDto
        {
            TotalAmount = total,
            ExpenseCount = count,
            AverageAmount = count > 0 ? Math.Round(total / count, 2) : 0,
            ByItem = byItem
        };
    }
}
