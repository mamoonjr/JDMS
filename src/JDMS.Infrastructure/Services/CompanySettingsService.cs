using System.Reflection;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Services;

public class CompanySettingsService : ICompanySettingsService
{
    public const string DefaultLogoRelativePath = "/images/company/logo.png";
    private readonly IUnitOfWork _unitOfWork;

    public CompanySettingsService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<CompanyBrandingDto> GetBrandingAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return MapBranding(settings);
    }

    public async Task<CompanySettingsEditDto> GetForEditAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return new CompanySettingsEditDto
        {
            CompanyName = settings.CompanyName,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            TaxRate = settings.TaxRate,
            CurrentLogoUrl = ResolveLogoUrl(settings.LogoPath)
        };
    }

    public async Task UpdateAsync(CompanySettingsEditDto dto, CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.CompanyName = dto.CompanyName.Trim();
        settings.Address = dto.Address?.Trim();
        settings.Phone = dto.Phone?.Trim();
        settings.Email = dto.Email?.Trim();
        settings.TaxRate = dto.TaxRate;
        settings.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CompanySettings.UpdateAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLogoPathAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.LogoPath = relativePath;
        settings.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CompanySettings.UpdateAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public string ResolvePhysicalLogoPath(string? logoPath, string webRootPath)
    {
        var relative = (logoPath ?? DefaultLogoRelativePath).TrimStart('~').TrimStart('/');
        var physical = Path.Combine(webRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(physical) ? physical : Path.Combine(webRootPath, "images", "company", "logo.png");
    }

    private async Task<CompanySettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.CompanySettings.Query().FirstOrDefaultAsync(cancellationToken);
        if (settings != null) return settings;

        settings = new CompanySettings
        {
            CompanyName = "نظام إدارة التوصيل الأردني - JDMS",
            LogoPath = DefaultLogoRelativePath,
            Address = "عمان، المملكة الأردنية الهاشمية",
            Phone = "0790000000",
            Email = "info@jdms.jo",
            TaxRate = 0.16m
        };
        await _unitOfWork.CompanySettings.AddAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static CompanyBrandingDto MapBranding(CompanySettings settings)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return new CompanyBrandingDto
        {
            CompanyName = settings.CompanyName,
            LogoUrl = ResolveLogoUrl(settings.LogoPath),
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            SystemVersion = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0"
        };
    }

    private static string ResolveLogoUrl(string? logoPath) =>
        string.IsNullOrWhiteSpace(logoPath) ? DefaultLogoRelativePath : logoPath;
}
