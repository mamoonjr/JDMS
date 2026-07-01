using JDMS.Domain.Entities;
using JDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DeliveryTracking> DeliveryTrackings => Set<DeliveryTracking>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.CustomerCode).IsUnique();
            e.HasIndex(x => x.MobileNumber).IsUnique();
            e.Property(x => x.CustomerCode).HasMaxLength(20);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.MobileNumber).HasMaxLength(20).IsRequired();
        });

        builder.Entity<Governorate>(e =>
        {
            e.HasIndex(x => x.NameAr).IsUnique();
            e.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Area>(e =>
        {
            e.HasOne(x => x.Governorate).WithMany(g => g.Areas).HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.GovernorateId, x.NameAr });
            e.Property(x => x.DeliveryFee).HasPrecision(18, 2);
        });

        builder.Entity<Address>(e =>
        {
            e.Property(x => x.Neighborhood).HasMaxLength(150).IsRequired();
            e.Property(x => x.DeliveryNotes).HasMaxLength(500);
            e.HasOne(x => x.Customer).WithMany(c => c.Addresses).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Governorate).WithMany().HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Area).WithMany(a => a.Addresses).HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(e =>
        {
            e.HasIndex(x => x.SKU).IsUnique();
            e.HasIndex(x => x.Barcode);
            e.Property(x => x.SKU).HasMaxLength(50).IsRequired();
            e.Property(x => x.Barcode).HasMaxLength(50);
            e.Property(x => x.ImagePath).HasMaxLength(500);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<Order>(e =>
        {
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasIndex(x => x.OrderDate);
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.Customer).WithMany(c => c.Orders).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Address).WithMany(a => a.Orders).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedDriver).WithMany().HasForeignKey(x => x.AssignedDriverId).OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.AmountReceived).HasPrecision(18, 2);
            e.Property(x => x.ChangeDue).HasPrecision(18, 2);
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.DeliveryFee).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.Tax).HasPrecision(18, 2);
            e.Property(x => x.GrandTotal).HasPrecision(18, 2);
            e.Property(x => x.Notes).HasMaxLength(500);
        });

        builder.Entity<OrderDetail>(e =>
        {
            e.HasOne(x => x.Order).WithMany(o => o.OrderDetails).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.OrderDetails).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        builder.Entity<Driver>(e =>
        {
            e.HasOne(x => x.AssignedArea).WithMany(a => a.Drivers).HasForeignKey(x => x.AssignedAreaId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<DeliveryTracking>(e =>
        {
            e.HasOne(x => x.Order).WithOne(o => o.DeliveryTracking).HasForeignKey<DeliveryTracking>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Driver).WithMany(d => d.DeliveryTrackings).HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasOne(x => x.Order).WithOne(o => o.Invoice).HasForeignKey<Invoice>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.DeliveryFee).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.Tax).HasPrecision(18, 2);
            e.Property(x => x.GrandTotal).HasPrecision(18, 2);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(x => x.ActionDate);
            e.HasIndex(x => x.UserId);
        });

        builder.Entity<CompanySettings>(e =>
        {
            e.Property(x => x.TaxRate).HasPrecision(5, 4);
        });

        builder.Entity<Expense>(e =>
        {
            e.HasIndex(x => x.StartDate);
            e.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Notes).HasMaxLength(500);
        });
    }
}
