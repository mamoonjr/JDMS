using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using JDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Services;

public class PosService : IPosService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IInvoiceService _invoiceService;

    public PosService(ApplicationDbContext context, IAuditService auditService, IInvoiceService invoiceService)
    {
        _context = context;
        _auditService = auditService;
        _invoiceService = invoiceService;
    }

    public async Task<CustomerLookupDto?> LookupByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        phone = NormalizePhone(phone);
        if (string.IsNullOrEmpty(phone)) return null;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MobileNumber == phone, cancellationToken);

        if (customer == null) return new CustomerLookupDto { Found = false };

        var lastAddress = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customer.Id)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new CustomerLookupDto
        {
            Found = true,
            CustomerId = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.MobileNumber,
            SecondaryPhone = customer.SecondaryMobile,
            GovernorateId = lastAddress?.GovernorateId,
            AreaId = lastAddress?.AreaId,
            Neighborhood = lastAddress?.Neighborhood,
            BuildingNumber = lastAddress?.Building,
            Street = lastAddress?.Street
        };
    }

    public async Task<IReadOnlyList<PosProductDto>> SearchProductsAsync(string? query, int limit = 40, CancellationToken cancellationToken = default)
    {
        var q = _context.Products.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p =>
                p.ProductName.Contains(term) ||
                p.SKU.Contains(term) ||
                (p.Barcode != null && p.Barcode.Contains(term)));
        }

        return await q
            .OrderBy(p => p.ProductName)
            .Take(limit)
            .Select(p => new PosProductDto
            {
                Id = p.Id,
                Name = p.ProductName,
                Sku = p.SKU,
                Barcode = p.Barcode,
                UnitPrice = p.UnitPrice,
                ImageUrl = string.IsNullOrEmpty(p.ImagePath) ? "/images/products/default.svg" : p.ImagePath
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PosStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var pendingStatuses = new[]
        {
            OrderStatus.New,
            OrderStatus.Processing,
            OrderStatus.ReadyForDelivery,
            OrderStatus.OutForDelivery
        };

        var ordersToday = await _context.Orders.CountAsync(o => o.OrderDate >= today && o.OrderDate < tomorrow, cancellationToken);
        var revenueToday = await _context.Orders
            .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.GrandTotal, cancellationToken);

        var pending = await _context.Orders.CountAsync(o => pendingStatuses.Contains(o.Status), cancellationToken);
        var drivers = await _context.Drivers.CountAsync(d => d.IsActive, cancellationToken);

        return new PosStatsDto
        {
            OrdersToday = ordersToday,
            RevenueToday = revenueToday,
            PendingDeliveries = pending,
            ActiveDrivers = drivers
        };
    }

    public async Task<decimal?> GetAreaDeliveryFeeAsync(int areaId, CancellationToken cancellationToken = default)
    {
        var area = await _context.Areas.AsNoTracking()
            .Where(a => a.Id == areaId)
            .Select(a => (decimal?)a.DeliveryFee)
            .FirstOrDefaultAsync(cancellationToken);
        return area;
    }

    public async Task<PosSubmitResult> SubmitAsync(PosSubmitModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.PhoneNumber))
            return Fail("الاسم ورقم الهاتف مطلوبان");

        if (model.GovernorateId <= 0 || model.AreaId <= 0)
            return Fail("يرجى اختيار المحافظة والمنطقة");

        if (string.IsNullOrWhiteSpace(model.Neighborhood) || string.IsNullOrWhiteSpace(model.BuildingNumber))
            return Fail("الحي ورقم المبنى مطلوبان");

        var lines = model.Lines?.Where(l => l.ProductId > 0 && l.Quantity > 0).ToList() ?? new();
        if (lines.Count == 0)
            return Fail("يجب إضافة منتج واحد على الأقل");

        var area = await _context.Areas.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == model.AreaId, cancellationToken);
        if (area == null) return Fail("المنطقة غير موجودة");

        if (model.AssignedDriverId.HasValue)
        {
            var driverExists = await _context.Drivers.AnyAsync(d => d.Id == model.AssignedDriverId && d.IsActive, cancellationToken);
            if (!driverExists) return Fail("السائق غير موجود أو غير نشط");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var phone = NormalizePhone(model.PhoneNumber);
            Customer customer;

            if (model.ExistingCustomerId.HasValue && model.ExistingCustomerId > 0)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == model.ExistingCustomerId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("العميل غير موجود");
                customer.FullName = model.FullName.Trim();
                customer.MobileNumber = phone;
                customer.SecondaryMobile = model.SecondaryPhone?.Trim();
            }
            else
            {
                var existing = await _context.Customers
                    .FirstOrDefaultAsync(c => c.MobileNumber == phone, cancellationToken);
                if (existing != null)
                {
                    customer = existing;
                    customer.FullName = model.FullName.Trim();
                    customer.SecondaryMobile = model.SecondaryPhone?.Trim();
                }
                else
                {
                    var count = await _context.Customers.CountAsync(cancellationToken);
                    customer = new Customer
                    {
                        CustomerCode = $"CUS-{(count + 1):D6}",
                        FullName = model.FullName.Trim(),
                        MobileNumber = phone,
                        SecondaryMobile = model.SecondaryPhone?.Trim()
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            var deliveryNotes = Truncate(model.DeliveryNotes, 500);
            var address = new Address
            {
                CustomerId = customer.Id,
                GovernorateId = model.GovernorateId,
                AreaId = model.AreaId,
                Neighborhood = model.Neighborhood.Trim(),
                Building = model.BuildingNumber.Trim(),
                Street = model.Street?.Trim() ?? string.Empty,
                DeliveryNotes = deliveryNotes,
                IsDefault = false
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(cancellationToken);

            var settings = await _context.CompanySettings.FirstOrDefaultAsync(cancellationToken);
            var taxRate = settings?.TaxRate ?? 0.16m;
            var subtotal = lines.Sum(l => l.UnitPrice * l.Quantity);
            var deliveryFee = Math.Max(0, model.DeliveryFee);
            var discount = model.Discount;
            var taxable = Math.Max(0, subtotal - discount);
            var tax = Math.Round(taxable * taxRate, 2);
            var grandTotal = subtotal + deliveryFee - discount + tax;

            var orderNotes = Truncate(model.Notes, 500);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customer.Id,
                AddressId = address.Id,
                OrderDate = DateTime.UtcNow,
                Status = model.Status,
                Notes = orderNotes,
                PaymentMethod = model.PaymentMethod,
                AmountReceived = Math.Max(0, model.AmountReceived),
                ChangeDue = Math.Max(0, model.ChangeDue),
                AssignedDriverId = model.AssignedDriverId,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                Discount = discount,
                Tax = tax,
                GrandTotal = grandTotal
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var line in lines)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    Discount = 0,
                    LineTotal = line.UnitPrice * line.Quantity
                });
            }

            if (model.AssignedDriverId.HasValue)
            {
                _context.DeliveryTrackings.Add(new DeliveryTracking
                {
                    OrderId = order.Id,
                    DriverId = model.AssignedDriverId.Value,
                    AssignDate = DateTime.UtcNow,
                    Status = DeliveryStatus.Assigned,
                    DeliveryNotes = deliveryNotes
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditService.LogAsync(AuditActionType.OrderCreated, "Order", order.Id.ToString(),
                cancellationToken: cancellationToken);

            int? invoiceId = null;
            if (model.CreateInvoice)
                invoiceId = await _invoiceService.CreateInvoiceForOrderAsync(order.Id, cancellationToken);

            return new PosSubmitResult
            {
                Success = true,
                Message = "تم حفظ الطلب بنجاح",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                InvoiceId = invoiceId
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Fail($"حدث خطأ أثناء الحفظ: {ex.Message}");
        }
    }

    private static PosSubmitResult Fail(string message) =>
        new() { Success = false, Message = message };

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        return t.Length <= max ? t : t[..max];
    }

    private static string NormalizePhone(string phone) =>
        new string(phone.Where(char.IsDigit).ToArray());

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
