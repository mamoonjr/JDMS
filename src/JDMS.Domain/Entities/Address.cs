using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Address : BaseEntity
{
    public int CustomerId { get; set; }
    public int GovernorateId { get; set; }
    public int AreaId { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? Apartment { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? GoogleMapsLink { get; set; }
    public bool IsDefault { get; set; }

    public Customer Customer { get; set; } = null!;
    public Governorate Governorate { get; set; } = null!;
    public Area Area { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
