using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class CompanySettings : BaseEntity
{
    public string CompanyName { get; set; } = "نظام إدارة التوصيل الأردني";
    public string? LogoPath { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal TaxRate { get; set; } = 0.16m;
}
