using JDMS.Application.Constants;
using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
public class AuditLogsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var logs = await _unitOfWork.AuditLogs.Query()
            .AsNoTracking()
            .OrderByDescending(l => l.ActionDate)
            .Take(500)
            .ToListAsync(cancellationToken);
        return View(logs);
    }
}
