using JDMS.Domain.Common;
using JDMS.Domain.Enums;

namespace JDMS.Domain.Entities;

public class Driver : BaseEntity
{
    public string DriverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public int? AssignedAreaId { get; set; }
    public bool IsActive { get; set; } = true;

    public Area? AssignedArea { get; set; }
    public ICollection<DeliveryTracking> DeliveryTrackings { get; set; } = new List<DeliveryTracking>();
}
