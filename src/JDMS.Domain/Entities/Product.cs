using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Product : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? ImagePath { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
