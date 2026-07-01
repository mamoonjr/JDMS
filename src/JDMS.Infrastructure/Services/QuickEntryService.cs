using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using JDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Services;

public class QuickEntryService : IQuickEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public QuickEntryService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
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

    public async Task<QuickEntrySubmitResult> SubmitAsync(QuickEntryViewModel model, CancellationToken cancellationToken = default)
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

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
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

                var address = new Address
                {
                    CustomerId = customer.Id,
                    GovernorateId = model.GovernorateId,
                    AreaId = model.AreaId,
                    Neighborhood = model.Neighborhood.Trim(),
                    Building = model.BuildingNumber.Trim(),
                    Street = model.Street?.Trim() ?? string.Empty,
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

                var orderNotes = string.IsNullOrWhiteSpace(model.Notes)
                    ? null
                    : model.Notes.Trim().Length <= 500 ? model.Notes.Trim() : model.Notes.Trim()[..500];

                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    CustomerId = customer.Id,
                    AddressId = address.Id,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.New,
                    Notes = orderNotes,
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
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _auditService.LogAsync(AuditActionType.OrderCreated, "Order", order.Id.ToString(),
                    cancellationToken: cancellationToken);

                return new QuickEntrySubmitResult
                {
                    Success = true,
                    Message = "تم حفظ الطلب بنجاح",
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Fail($"حدث خطأ أثناء الحفظ: {ex.Message}");
            }
        });
    }

    private static QuickEntrySubmitResult Fail(string message) =>
        new() { Success = false, Message = message };

    private static string NormalizePhone(string phone) =>
        new string(phone.Where(char.IsDigit).ToArray());

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
