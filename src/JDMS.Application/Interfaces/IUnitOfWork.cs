using JDMS.Domain.Entities;

namespace JDMS.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Customer> Customers { get; }
    IRepository<Governorate> Governorates { get; }
    IRepository<Area> Areas { get; }
    IRepository<Address> Addresses { get; }
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderDetail> OrderDetails { get; }
    IRepository<Driver> Drivers { get; }
    IRepository<DeliveryTracking> DeliveryTrackings { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<CompanySettings> CompanySettings { get; }
    IRepository<Expense> Expenses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
