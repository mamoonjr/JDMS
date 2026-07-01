using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Area : BaseEntity
{
    public int GovernorateId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }
    public bool IsActive { get; set; } = true;

    public Governorate Governorate { get; set; } = null!;
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
