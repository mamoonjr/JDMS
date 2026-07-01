using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface ICompanySettingsService
{
    Task<CompanyBrandingDto> GetBrandingAsync(CancellationToken cancellationToken = default);
    Task<CompanySettingsEditDto> GetForEditAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(CompanySettingsEditDto dto, CancellationToken cancellationToken = default);
    Task UpdateLogoPathAsync(string relativePath, CancellationToken cancellationToken = default);
    string ResolvePhysicalLogoPath(string? logoPath, string webRootPath);
}
