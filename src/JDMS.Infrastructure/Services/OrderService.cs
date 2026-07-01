using ClosedXML.Excel;
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
        => await GetFilteredAsync(new OrderFilterDto { Search = search }, cancellationToken);

    public async Task<IReadOnlyList<OrderListDto>> GetFilteredAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
    {
        var orders = await BuildFilteredQuery(filter)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToListDto).ToList();
    }

    public async Task<byte[]> ExportExcelAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
    {
        var orders = await BuildFilteredQuery(filter)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        var summary = workbook.Worksheets.Add("ملخص الطلبات");
        var summaryHeaders = new[]
        {
            "رقم الطلب", "العميل", "الجوال", "تاريخ الطلب", "تاريخ التوصيل", "الحالة", "طريقة الدفع",
            "العنوان", "ملخص المنتجات", "عدد الأصناف", "المجموع الفرعي", "التوصيل", "الخصم", "الضريبة", "الإجمالي", "ملاحظات"
        };
        for (var c = 0; c < summaryHeaders.Length; c++)
            summary.Cell(1, c + 1).Value = summaryHeaders[c];

        var summaryHeaderRow = summary.Range(1, 1, 1, summaryHeaders.Length);
        summaryHeaderRow.Style.Font.Bold = true;
        summaryHeaderRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (var i = 0; i < orders.Count; i++)
        {
            var o = orders[i];
            var dto = MapToListDto(o);
            var row = i + 2;
            summary.Cell(row, 1).Value = dto.OrderNumber;
            summary.Cell(row, 2).Value = dto.CustomerName;
            summary.Cell(row, 3).Value = dto.CustomerPhone ?? "";
            summary.Cell(row, 4).Value = dto.OrderDate.ToString("yyyy-MM-dd HH:mm");
            summary.Cell(row, 5).Value = dto.DeliveryDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
            summary.Cell(row, 6).Value = dto.StatusName;
            summary.Cell(row, 7).Value = dto.PaymentMethodName ?? "";
            summary.Cell(row, 8).Value = dto.AddressSummary ?? "";
            summary.Cell(row, 9).Value = dto.ProductsSummary ?? "";
            summary.Cell(row, 10).Value = dto.ItemCount;
            summary.Cell(row, 11).Value = dto.Subtotal;
            summary.Cell(row, 12).Value = dto.DeliveryFee;
            summary.Cell(row, 13).Value = dto.Discount;
            summary.Cell(row, 14).Value = dto.Tax;
            summary.Cell(row, 15).Value = dto.GrandTotal;
            summary.Cell(row, 16).Value = dto.Notes ?? "";
        }
        summary.Columns().AdjustToContents();

        var lines = workbook.Worksheets.Add("تفاصيل المنتجات");
        var lineHeaders = new[] { "رقم الطلب", "العميل", "المنتج", "الكمية", "السعر", "خصم", "إجمالي السطر", "حالة الطلب" };
        for (var c = 0; c < lineHeaders.Length; c++)
            lines.Cell(1, c + 1).Value = lineHeaders[c];

        var linesHeaderRow = lines.Range(1, 1, 1, lineHeaders.Length);
        linesHeaderRow.Style.Font.Bold = true;
        linesHeaderRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        var lineRow = 2;
        foreach (var order in orders)
        {
            var statusName = GetStatusName(order.Status);
            foreach (var detail in order.OrderDetails.OrderBy(d => d.Id))
            {
                lines.Cell(lineRow, 1).Value = order.OrderNumber;
                lines.Cell(lineRow, 2).Value = order.Customer.FullName;
                lines.Cell(lineRow, 3).Value = detail.Product.ProductName;
                lines.Cell(lineRow, 4).Value = detail.Quantity;
                lines.Cell(lineRow, 5).Value = detail.UnitPrice;
                lines.Cell(lineRow, 6).Value = detail.Discount;
                lines.Cell(lineRow, 7).Value = detail.LineTotal;
                lines.Cell(lineRow, 8).Value = statusName;
                lineRow++;
            }
        }
        lines.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private IQueryable<Order> BuildFilteredQuery(OrderFilterDto filter)
    {
        var query = _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Address).ThenInclude(a => a.Governorate)
            .Include(o => o.Address).ThenInclude(a => a.Area)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(o =>
                o.OrderNumber.Contains(term) ||
                o.Customer.FullName.Contains(term) ||
                o.Customer.MobileNumber.Contains(term));
        }

        if (filter.Status.HasValue)
            query = query.Where(o => (int)o.Status == filter.Status.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.OrderDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(o => o.OrderDate < filter.DateTo.Value.Date.AddDays(1));

        return query;
    }

    private OrderListDto MapToListDto(Order o)
    {
        var products = o.OrderDetails
            .OrderBy(d => d.Id)
            .Select(d => $"{d.Product.ProductName} ×{d.Quantity}")
            .ToList();

        var addressParts = new List<string>();
        if (o.Address.Governorate != null) addressParts.Add(o.Address.Governorate.NameAr);
        if (o.Address.Area != null) addressParts.Add(o.Address.Area.NameAr);
        if (!string.IsNullOrWhiteSpace(o.Address.Neighborhood)) addressParts.Add(o.Address.Neighborhood);
        if (!string.IsNullOrWhiteSpace(o.Address.Building)) addressParts.Add(o.Address.Building);

        return new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.Customer.FullName,
            CustomerPhone = o.Customer.MobileNumber,
            OrderDate = o.OrderDate,
            DeliveryDate = o.DeliveryDate,
            Status = (int)o.Status,
            StatusName = GetStatusName(o.Status),
            PaymentMethodName = GetPaymentMethodName(o.PaymentMethod),
            AddressSummary = addressParts.Count > 0 ? string.Join(" - ", addressParts) : null,
            ProductsSummary = products.Count > 0 ? string.Join("، ", products) : null,
            ItemCount = o.OrderDetails.Sum(d => d.Quantity),
            Subtotal = o.Subtotal,
            DeliveryFee = o.DeliveryFee,
            Discount = o.Discount,
            Tax = o.Tax,
            GrandTotal = o.GrandTotal,
            Notes = o.Notes
        };
    }

    private static string GetPaymentMethodName(PaymentMethod method) => method switch
    {
        PaymentMethod.CreditCard => "بطاقة ائتمان",
        PaymentMethod.BankTransfer => "تحويل بنكي",
        PaymentMethod.OnlinePayment => "دفع إلكتروني",
        _ => "نقدي"
    };

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
            StatusName = GetStatusName(order.Status),
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
