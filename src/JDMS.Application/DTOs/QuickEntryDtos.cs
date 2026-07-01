using System.ComponentModel.DataAnnotations;

namespace JDMS.Application.DTOs;

public class QuickEntryViewModel
{
    public int? ExistingCustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public int GovernorateId { get; set; }
    public int AreaId { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string? Street { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
    public List<QuickEntryLineDto> Lines { get; set; } = new() { new() };
}

public class QuickEntryLineDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class QuickEntrySubmitResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

public class CustomerLookupDto
{
    public bool Found { get; set; }
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public int? GovernorateId { get; set; }
    public int? AreaId { get; set; }
    public string? Neighborhood { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
}

public class QuickEntryLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? DeliveryFee { get; set; }
    public decimal? UnitPrice { get; set; }
}
