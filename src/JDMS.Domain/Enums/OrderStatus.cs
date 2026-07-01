namespace JDMS.Domain.Enums;

public enum OrderStatus
{
    New = 0,
    Processing = 1,
    ReadyForDelivery = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6
}
