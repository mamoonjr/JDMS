namespace JDMS.Application.DTOs;

public class CompanyBrandingDto
{
    public string CompanyName { get; set; } = "نظام إدارة التوصيل الأردني";
    public string LogoUrl { get; set; } = "/images/company/logo.png";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string SystemVersion { get; set; } = "1.0.0";
}
