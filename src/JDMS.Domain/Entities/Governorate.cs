using JDMS.Domain.Common;

namespace JDMS.Domain.Entities;

public class Governorate : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Area> Areas { get; set; } = new List<Area>();
}
