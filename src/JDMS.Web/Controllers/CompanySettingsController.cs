using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JDMS.Web.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class CompanySettingsController : Controller
{
    private readonly ICompanySettingsService _companySettingsService;
    private readonly IWebHostEnvironment _environment;

    public CompanySettingsController(ICompanySettingsService companySettingsService, IWebHostEnvironment environment)
    {
        _companySettingsService = companySettingsService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await _companySettingsService.GetForEditAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CompanySettingsEditDto model, IFormFile? logoFile, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (logoFile is { Length: > 0 })
        {
            if (logoFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "حجم الشعار يجب ألا يتجاوز 2 ميجابايت");
                return View(model);
            }

            var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            if (ext is not ".png" and not ".jpg" and not ".jpeg" and not ".webp")
            {
                ModelState.AddModelError(string.Empty, "صيغة الشعار غير مدعومة. استخدم PNG أو JPG أو WEBP.");
                return View(model);
            }

            var storedName = ext == ".png" ? "logo.png" : $"logo{ext}";
            var relativePath = $"/images/company/{storedName}";
            var companyDir = Path.Combine(_environment.WebRootPath, "images", "company");
            Directory.CreateDirectory(companyDir);
            var physicalPath = Path.Combine(companyDir, storedName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream, cancellationToken);
            }

            await _companySettingsService.UpdateLogoPathAsync(relativePath, cancellationToken);
            model.CurrentLogoUrl = relativePath;
        }

        await _companySettingsService.UpdateAsync(model, cancellationToken);
        TempData["Success"] = "تم حفظ إعدادات الشركة بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
