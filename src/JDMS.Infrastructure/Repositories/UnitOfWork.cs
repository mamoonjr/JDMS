using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Infrastructure.Data;

namespace JDMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<Customer>? _customers;
    private IRepository<Governorate>? _governorates;
    private IRepository<Area>? _areas;
    private IRepository<Address>? _addresses;
    private IRepository<Product>? _products;
    private IRepository<Order>? _orders;
    private IRepository<OrderDetail>? _orderDetails;
    private IRepository<Driver>? _drivers;
    private IRepository<DeliveryTracking>? _deliveryTrackings;
    private IRepository<Invoice>? _invoices;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<CompanySettings>? _companySettings;
    private IRepository<Expense>? _expenses;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);
    public IRepository<Governorate> Governorates => _governorates ??= new Repository<Governorate>(_context);
    public IRepository<Area> Areas => _areas ??= new Repository<Area>(_context);
    public IRepository<Address> Addresses => _addresses ??= new Repository<Address>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<OrderDetail> OrderDetails => _orderDetails ??= new Repository<OrderDetail>(_context);
    public IRepository<Driver> Drivers => _drivers ??= new Repository<Driver>(_context);
    public IRepository<DeliveryTracking> DeliveryTrackings => _deliveryTrackings ??= new Repository<DeliveryTracking>(_context);
    public IRepository<Invoice> Invoices => _invoices ??= new Repository<Invoice>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);
    public IRepository<CompanySettings> CompanySettings => _companySettings ??= new Repository<CompanySettings>(_context);
    public IRepository<Expense> Expenses => _expenses ??= new Repository<Expense>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
