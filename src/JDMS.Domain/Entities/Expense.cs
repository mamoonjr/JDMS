using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Expense : BaseEntity
{
    /// <summary>اسم المصروف أو المنتج.</summary>
    public string ItemName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    /// <summary>تاريخ بداية المصروف.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>تاريخ نهاية المصروف (اختياري).</summary>
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
