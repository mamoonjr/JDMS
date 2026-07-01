using System.ComponentModel.DataAnnotations;
using JDMS.Domain.Enums;

namespace JDMS.Application.DTOs;

public class PosSubmitModel
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
    public string? DeliveryNotes { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public int? AssignedDriverId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal AmountReceived { get; set; }
    public decimal ChangeDue { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
    public List<PosLineDto> Lines { get; set; } = new();
    public bool CreateInvoice { get; set; }
}

public class PosLineDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class PosSubmitResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? InvoiceId { get; set; }
}

public class PosProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }
    public string ImageUrl { get; set; } = "/images/products/default.png";
}

public class PosStatsDto
{
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public int PendingDeliveries { get; set; }
    public int ActiveDrivers { get; set; }
}

public class PosAreaDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }
}

public class PosDriverDto
{
    public int Id { get; set; }
    public string DriverName { get; set; } = string.Empty;
}
