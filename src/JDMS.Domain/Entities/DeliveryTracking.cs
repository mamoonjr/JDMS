using JDMS.Domain.Common;
using JDMS.Domain.Enums;

namespace JDMS.Domain.Entities;

public class DeliveryTracking : BaseEntity
{
    public int OrderId { get; set; }
    public int DriverId { get; set; }
    public DateTime AssignDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Assigned;
    public string? DeliveryNotes { get; set; }

    public Order Order { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
}
