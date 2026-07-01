using System.ComponentModel.DataAnnotations;

namespace JDMS.Application.DTOs;

public class CompanySettingsEditDto
{
    [Required(ErrorMessage = "اسم الشركة مطلوب")]
    [MaxLength(200)]
    [Display(Name = "اسم الشركة")]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [MaxLength(50)]
    [Display(Name = "الهاتف")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Range(0, 1)]
    [Display(Name = "نسبة الضريبة")]
    public decimal TaxRate { get; set; } = 0.16m;

    public string? CurrentLogoUrl { get; set; }
}
