using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Enums;
using JDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuditService _auditService;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuditService auditService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var list = new List<UserViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new UserViewModel
            {
                Id = u.Id, UserName = u.UserName!, Email = u.Email!, FullName = u.FullName,
                Role = roles.FirstOrDefault() ?? "", IsActive = u.IsActive
            });
        }
        return View(list);
    }

    public IActionResult Create() => View(new UserCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = model.UserName, Email = model.Email, FullName = model.FullName, EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }
        await _userManager.AddToRoleAsync(user, model.Role);
        await _auditService.LogAsync(AuditActionType.UserCreated, "User", user.Id, cancellationToken: cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            await _auditService.LogAsync(AuditActionType.UserUpdated, "User", id, cancellationToken: cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }
}
