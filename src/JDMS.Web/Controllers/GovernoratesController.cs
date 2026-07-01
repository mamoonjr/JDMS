using JDMS.Application.Constants;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
public class GovernoratesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public GovernoratesController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Governorates.Query()
            .Include(g => g.Areas)
            .AsNoTracking()
            .OrderBy(g => g.NameAr)
            .ToListAsync(cancellationToken);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArea(int governorateId, string nameAr, decimal deliveryFee, CancellationToken cancellationToken)
    {
        await _unitOfWork.Areas.AddAsync(new Area
        {
            GovernorateId = governorateId,
            NameAr = nameAr,
            NameEn = nameAr,
            DeliveryFee = deliveryFee
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAreaFee(int areaId, decimal deliveryFee, CancellationToken cancellationToken)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(areaId, cancellationToken);
        if (area != null)
        {
            area.DeliveryFee = deliveryFee;
            await _unitOfWork.Areas.UpdateAsync(area, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetAreas(int governorateId, CancellationToken cancellationToken)
    {
        var areas = await _unitOfWork.Areas.Query()
            .Where(a => a.GovernorateId == governorateId && a.IsActive)
            .Select(a => new { a.Id, a.NameAr, a.DeliveryFee })
            .ToListAsync(cancellationToken);
        return Json(areas);
    }
}
