namespace JDMS.Domain.Enums;

public enum AuditActionType
{
    Login = 0,
    Logout = 1,
    OrderCreated = 2,
    OrderUpdated = 3,
    OrderStatusChanged = 4,
    UserCreated = 5,
    UserUpdated = 6,
    UserDeleted = 7,
    CustomerCreated = 8,
    CustomerUpdated = 9,
    CustomerDeleted = 10,
    Other = 99
}
