using JDMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JDMS.Web.ViewComponents;

public class CompanyBrandingViewComponent : ViewComponent
{
    private readonly ICompanySettingsService _companySettingsService;

    public CompanyBrandingViewComponent(ICompanySettingsService companySettingsService)
    {
        _companySettingsService = companySettingsService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string variant = "header")
    {
        var model = await _companySettingsService.GetBrandingAsync();
        return View(variant, model);
    }
}
