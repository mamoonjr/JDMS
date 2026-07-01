using System.ComponentModel.DataAnnotations;

namespace JDMS.Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseCreateDto
{
    [Required(ErrorMessage = "اسم المصروف مطلوب")]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class ExpenseFilterDto
{
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }

    public string? Search { get; set; }
}

public class ExpenseStatsDto
{
    public decimal TotalAmount { get; set; }
    public int ExpenseCount { get; set; }
    public decimal AverageAmount { get; set; }
    public List<ExpenseItemSummaryDto> ByItem { get; set; } = new();
}

public class ExpenseItemSummaryDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ExpenseIndexViewModel
{
    public ExpenseFilterDto Filter { get; set; } = new();
    public List<ExpenseDto> Expenses { get; set; } = new();
    public ExpenseStatsDto Stats { get; set; } = new();
}
