using JDMS.Application.Constants;
using JDMS.Application.Interfaces;
using JDMS.Domain.Enums;
using JDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JDMS.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userName, string password, string? returnUrl, CancellationToken cancellationToken)
    {
        userName = userName?.Trim() ?? string.Empty;
        password = password?.Trim() ?? string.Empty;

        var user = await _userManager.FindByNameAsync(userName)
            ?? await _userManager.FindByEmailAsync(userName);

        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة");
            return View();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة");
            return View();
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        await _auditService.LogAsync(AuditActionType.Login, "User", user.Id, cancellationToken: cancellationToken);
        return Redirect(returnUrl ?? "/Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _auditService.LogAsync(AuditActionType.Logout, "User", cancellationToken: cancellationToken);
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
