using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? SecondaryMobile { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
