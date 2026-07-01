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
    private readonly IAuditService _auditService;

    public UsersController(UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync(cancellationToken);
        var list = new List<UserViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var role = roles.FirstOrDefault() ?? "";
            list.Add(new UserViewModel
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                FullName = u.FullName,
                Role = role,
                RoleDisplayAr = RoleLabels.ToArabic(role),
                IsActive = u.IsActive
            });
        }
        return View(list);
    }

    public IActionResult Create() => View(new UserCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!RoleLabels.IsValid(model.Role))
            ModelState.AddModelError(nameof(model.Role), "صلاحية غير صالحة");

        if (!ModelState.IsValid)
            return View(model);

        var email = string.IsNullOrWhiteSpace(model.Email)
            ? $"{model.UserName.Trim().ToLowerInvariant()}@jdms.jo"
            : model.Email.Trim();

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            Email = email,
            FullName = model.FullName.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, TranslateIdentityError(e.Description));
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        await _auditService.LogAsync(AuditActionType.UserCreated, "User", user.Id, cancellationToken: cancellationToken);
        TempData["Success"] = "تم إنشاء المستخدم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return View(new UserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? Roles.Employee,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();

        if (!RoleLabels.IsValid(model.Role))
            ModelState.AddModelError(nameof(model.Role), "صلاحية غير صالحة");

        var hasNewPassword = !string.IsNullOrWhiteSpace(model.NewPassword);
        if (hasNewPassword)
        {
            if (string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "تأكيد كلمة المرور مطلوب");
            else if (model.NewPassword != model.ConfirmNewPassword)
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "كلمة المرور غير متطابقة");
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId && !model.IsActive)
        {
            ModelState.AddModelError(string.Empty, "لا يمكنك تعطيل حسابك الحالي");
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.IsActive = model.IsActive;

        if (user.UserName != model.UserName.Trim())
        {
            var nameResult = await _userManager.SetUserNameAsync(user, model.UserName.Trim());
            if (!nameResult.Succeeded)
            {
                AddIdentityErrors(nameResult);
                return View(model);
            }
        }

        if (user.Email != model.Email.Trim())
        {
            var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
            if (!emailResult.Succeeded)
            {
                AddIdentityErrors(emailResult);
                return View(model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            if (user.Id == currentUserId && model.Role != Roles.Administrator)
            {
                ModelState.AddModelError(nameof(model.Role), "لا يمكنك إزالة صلاحية المدير من حسابك الحالي");
                return View(model);
            }
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        if (hasNewPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwdResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);
            if (!pwdResult.Succeeded)
            {
                foreach (var e in pwdResult.Errors)
                    ModelState.AddModelError(nameof(model.NewPassword), TranslateIdentityError(e.Description));
                return View(model);
            }
        }

        await _auditService.LogAsync(AuditActionType.UserUpdated, "User", user.Id, cancellationToken: cancellationToken);
        TempData["Success"] = hasNewPassword
            ? "تم تحديث المستخدم وكلمة المرور"
            : "تم تحديث المستخدم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, CancellationToken cancellationToken)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["Error"] = "لا يمكنك تعطيل حسابك الحالي";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            await _auditService.LogAsync(AuditActionType.UserUpdated, "User", id, cancellationToken: cancellationToken);
            TempData["Success"] = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم";
        }
        return RedirectToAction(nameof(Index));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, TranslateIdentityError(e.Description));
    }

    private static string TranslateIdentityError(string description) => description switch
    {
        var d when d.Contains("Passwords must have at least one digit") => "كلمة المرور يجب أن تحتوي رقماً واحداً على الأقل",
        var d when d.Contains("Passwords must have at least one lowercase") => "كلمة المرور يجب أن تحتوي حرفاً صغيراً",
        var d when d.Contains("Passwords must have at least one uppercase") => "كلمة المرور يجب أن تحتوي حرفاً كبيراً",
        var d when d.Contains("Passwords must be at least") => "كلمة المرور قصيرة جداً (6 أحرف على الأقل)",
        var d when d.Contains("is already taken") => "اسم الدخول أو البريد مستخدم مسبقاً",
        var d when d.Contains("InvalidEmail") || d.Contains("email") => "البريد الإلكتروني غير صالح",
        _ => description
    };
}
