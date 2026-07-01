using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public OrderService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    private static readonly Dictionary<OrderStatus, string> StatusNames = new()
    {
        { OrderStatus.New, "جديد" },
        { OrderStatus.Processing, "قيد المعالجة" },
        { OrderStatus.ReadyForDelivery, "جاهز للتوصيل" },
        { OrderStatus.OutForDelivery, "قيد التوصيل" },
        { OrderStatus.Delivered, "تم التسليم" },
        { OrderStatus.Cancelled, "ملغي" },
        { OrderStatus.Returned, "مرتجع" }
    };

    public string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    private static string? NormalizeOrderNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;
        var trimmed = notes.Trim();
        return trimmed.Length <= 500 ? trimmed : trimmed[..500];
    }

    public async Task<IReadOnlyList<OrderListDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.OrderNumber.Contains(search) || o.Customer.FullName.Contains(search));

        var rows = await query.OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                CustomerName = o.Customer.FullName,
                o.OrderDate,
                o.DeliveryDate,
                Status = (int)o.Status,
                o.GrandTotal
            })
            .ToListAsync(cancellationToken);

        return rows.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            OrderDate = o.OrderDate,
            DeliveryDate = o.DeliveryDate,
            Status = o.Status,
            StatusName = GetStatusName((OrderStatus)o.Status),
            GrandTotal = o.GrandTotal
        }).ToList();
    }

    private static string GetStatusName(OrderStatus status) =>
        StatusNames.GetValueOrDefault(status, status.ToString());

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Address).ThenInclude(a => a.Area)
            .Include(o => o.Address).ThenInclude(a => a.Governorate)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.FullName,
            AddressId = order.AddressId,
            AddressSummary = $"{order.Address.Governorate.NameAr} - {order.Address.Area.NameAr} - {order.Address.Street}",
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            Status = (int)order.Status,
            Notes = order.Notes,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Discount = order.Discount,
            Tax = order.Tax,
            GrandTotal = order.GrandTotal,
            Details = order.OrderDetails.Select(d => new OrderDetailLineDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product.ProductName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Discount = d.Discount,
                LineTotal = d.LineTotal
            }).ToList()
        };
    }

    public async Task<int> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var address = await _unitOfWork.Addresses.Query()
            .Include(a => a.Area)
            .FirstOrDefaultAsync(a => a.Id == dto.AddressId, cancellationToken)
            ?? throw new InvalidOperationException("العنوان غير موجود");

        var settings = await _unitOfWork.CompanySettings.Query().FirstOrDefaultAsync(cancellationToken);
        var taxRate = settings?.TaxRate ?? 0.16m;

        var subtotal = dto.Details.Sum(d => (d.UnitPrice * d.Quantity) - d.Discount);
        var deliveryFee = Math.Max(0, dto.DeliveryFee);
        var discount = dto.OrderDiscount;
        var taxable = Math.Max(0, subtotal - discount);
        var tax = Math.Round(taxable * taxRate, 2);
        var grandTotal = subtotal + deliveryFee - discount + tax;

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = dto.CustomerId,
            AddressId = dto.AddressId,
            OrderDate = DateTime.UtcNow,
            DeliveryDate = dto.DeliveryDate,
            Status = (OrderStatus)dto.Status,
            Notes = NormalizeOrderNotes(dto.Notes),
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Tax = tax,
            GrandTotal = grandTotal
        };

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var line in dto.Details)
        {
            var lineTotal = (line.UnitPrice * line.Quantity) - line.Discount;
            await _unitOfWork.OrderDetails.AddAsync(new OrderDetail
            {
                OrderId = order.Id,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.Discount,
                LineTotal = lineTotal
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(AuditActionType.OrderCreated, "Order", order.Id.ToString(), cancellationToken: cancellationToken);
        return order.Id;
    }

    public async Task UpdateAsync(int id, OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("الطلب غير موجود");

        var address = await _unitOfWork.Addresses.Query().Include(a => a.Area)
            .FirstOrDefaultAsync(a => a.Id == dto.AddressId, cancellationToken)!;
        var settings = await _unitOfWork.CompanySettings.Query().FirstOrDefaultAsync(cancellationToken);
        var taxRate = settings?.TaxRate ?? 0.16m;

        var existingDetails = await _unitOfWork.OrderDetails.FindAsync(d => d.OrderId == id, cancellationToken);
        foreach (var d in existingDetails)
            await _unitOfWork.OrderDetails.DeleteAsync(d, cancellationToken);

        var subtotal = dto.Details.Sum(d => (d.UnitPrice * d.Quantity) - d.Discount);
        var deliveryFee = Math.Max(0, dto.DeliveryFee);
        var discount = dto.OrderDiscount;
        var taxable = Math.Max(0, subtotal - discount);
        var tax = Math.Round(taxable * taxRate, 2);

        order.CustomerId = dto.CustomerId;
        order.AddressId = dto.AddressId;
        order.DeliveryDate = dto.DeliveryDate;
        order.Status = (OrderStatus)dto.Status;
        order.Notes = NormalizeOrderNotes(dto.Notes);
        order.Subtotal = subtotal;
        order.DeliveryFee = deliveryFee;
        order.Discount = discount;
        order.Tax = tax;
        order.GrandTotal = subtotal + deliveryFee - discount + tax;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);

        foreach (var line in dto.Details)
        {
            await _unitOfWork.OrderDetails.AddAsync(new OrderDetail
            {
                OrderId = order.Id,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.Discount,
                LineTotal = (line.UnitPrice * line.Quantity) - line.Discount
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(AuditActionType.OrderUpdated, "Order", id.ToString(), cancellationToken: cancellationToken);
    }

    public async Task UpdateStatusAsync(int id, int status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("الطلب غير موجود");
        var old = order.Status.ToString();
        order.Status = (OrderStatus)status;
        if (order.Status == OrderStatus.Delivered)
            order.DeliveryDate = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(AuditActionType.OrderStatusChanged, "Order", id.ToString(), old, order.Status.ToString(), cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("الطلب غير موجود");
        await _unitOfWork.Orders.DeleteAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
