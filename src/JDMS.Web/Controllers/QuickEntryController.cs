using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class QuickEntryController : Controller
{
    private readonly IQuickEntryService _quickEntryService;
    private readonly IUnitOfWork _unitOfWork;

    public QuickEntryController(IQuickEntryService quickEntryService, IUnitOfWork unitOfWork)
    {
        _quickEntryService = quickEntryService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(PosController.Index), "Pos");

    [HttpGet]
    public async Task<IActionResult> LookupCustomer(string phone, CancellationToken cancellationToken)
    {
        var result = await _quickEntryService.LookupByPhoneAsync(phone, cancellationToken);
        return Json(result ?? new CustomerLookupDto { Found = false });
    }

    [HttpGet]
    public async Task<IActionResult> GetAreas(int governorateId, CancellationToken cancellationToken)
    {
        var areas = await _unitOfWork.Areas.Query()
            .Where(a => a.GovernorateId == governorateId && a.IsActive)
            .OrderBy(a => a.NameAr)
            .Select(a => new { a.Id, a.NameAr })
            .ToListAsync(cancellationToken);
        return Json(areas);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] QuickEntryViewModel model, CancellationToken cancellationToken)
    {
        var result = await _quickEntryService.SubmitAsync(model, cancellationToken);
        return Json(result);
    }
}
