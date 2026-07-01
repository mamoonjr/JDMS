using JDMS.Domain.Common;
using JDMS.Domain.Enums;

namespace JDMS.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    /// <summary>ملاحظات الطلب (اختياري، حتى 500 حرف).</summary>
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal AmountReceived { get; set; }
    public decimal ChangeDue { get; set; }
    public int? AssignedDriverId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }

    public Customer Customer { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public Driver? AssignedDriver { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public DeliveryTracking? DeliveryTracking { get; set; }
    public Invoice? Invoice { get; set; }
}
