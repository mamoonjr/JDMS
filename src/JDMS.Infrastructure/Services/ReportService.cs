using ClosedXML.Excel;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JDMS.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private IQueryable<Domain.Entities.Order> FilterOrders(ReportFilterDto filter)
    {
        var q = _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Address).ThenInclude(a => a.Area)
            .Include(o => o.Address).ThenInclude(a => a.Governorate)
            .Include(o => o.DeliveryTracking).ThenInclude(d => d!.Driver)
            .AsQueryable();

        if (filter.DateFrom.HasValue)
            q = q.Where(o => o.OrderDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            q = q.Where(o => o.OrderDate <= filter.DateTo.Value.AddDays(1));
        if (filter.GovernorateId.HasValue)
            q = q.Where(o => o.Address.GovernorateId == filter.GovernorateId);
        if (filter.AreaId.HasValue)
            q = q.Where(o => o.Address.AreaId == filter.AreaId);
        if (filter.OrderStatus.HasValue)
            q = q.Where(o => (int)o.Status == filter.OrderStatus);
        if (filter.DriverId.HasValue)
            q = q.Where(o => o.DeliveryTracking != null && o.DeliveryTracking.DriverId == filter.DriverId);

        return q;
    }

    public async Task<ReportDataDto> GetOrdersReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var rows = await FilterOrders(filter).OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.OrderNumber,
                Customer = o.Customer.FullName,
                o.OrderDate,
                Status = o.Status.ToString(),
                o.GrandTotal
            }).ToListAsync(cancellationToken);

        return new ReportDataDto
        {
            Title = "تقرير الطلبات",
            TotalCount = rows.Count,
            TotalAmount = rows.Sum(r => r.GrandTotal),
            Rows = rows.Select(r => new Dictionary<string, object?>
            {
                ["رقم الطلب"] = r.OrderNumber,
                ["العميل"] = r.Customer,
                ["التاريخ"] = r.OrderDate.ToString("yyyy-MM-dd"),
                ["الحالة"] = r.Status,
                ["الإجمالي"] = r.GrandTotal
            }).ToList()
        };
    }

    public async Task<ReportDataDto> GetRevenueReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter.OrderStatus = (int)OrderStatus.Delivered;
        var data = await GetOrdersReportAsync(filter, cancellationToken);
        data.Title = "تقرير الإيرادات";
        return data;
    }

    public async Task<ReportDataDto> GetCustomersReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var q = _unitOfWork.Customers.Query();
        if (filter.DateFrom.HasValue)
            q = q.Where(c => c.CreatedAt >= filter.DateFrom);
        if (filter.DateTo.HasValue)
            q = q.Where(c => c.CreatedAt <= filter.DateTo);

        var rows = await q.OrderByDescending(c => c.CreatedAt).Select(c => new
        {
            c.CustomerCode,
            c.FullName,
            c.MobileNumber,
            c.Email,
            c.CreatedAt
        }).ToListAsync(cancellationToken);

        return new ReportDataDto
        {
            Title = "تقرير العملاء",
            TotalCount = rows.Count,
            Rows = rows.Select(r => new Dictionary<string, object?>
            {
                ["الرمز"] = r.CustomerCode,
                ["الاسم"] = r.FullName,
                ["الجوال"] = r.MobileNumber,
                ["البريد"] = r.Email,
                ["تاريخ الإنشاء"] = r.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList()
        };
    }

    public async Task<ReportDataDto> GetDriversReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var drivers = await _unitOfWork.Drivers.Query()
            .Include(d => d.AssignedArea)
            .Include(d => d.DeliveryTrackings)
            .ToListAsync(cancellationToken);

        return new ReportDataDto
        {
            Title = "تقرير السائقين",
            TotalCount = drivers.Count,
            Rows = drivers.Select(d => new Dictionary<string, object?>
            {
                ["الاسم"] = d.DriverName,
                ["الهاتف"] = d.PhoneNumber,
                ["المنطقة"] = d.AssignedArea?.NameAr,
                ["إجمالي التوصيلات"] = d.DeliveryTrackings.Count,
                ["المكتملة"] = d.DeliveryTrackings.Count(t => t.Status == DeliveryStatus.Delivered)
            }).ToList()
        };
    }

    public async Task<ReportDataDto> GetDeliveryReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var q = _unitOfWork.DeliveryTrackings.Query()
            .Include(t => t.Order).ThenInclude(o => o.Customer)
            .Include(t => t.Driver)
            .AsQueryable();

        if (filter.DateFrom.HasValue)
            q = q.Where(t => t.AssignDate >= filter.DateFrom);
        if (filter.DateTo.HasValue)
            q = q.Where(t => t.AssignDate <= filter.DateTo);
        if (filter.DriverId.HasValue)
            q = q.Where(t => t.DriverId == filter.DriverId);

        var rows = await q.OrderByDescending(t => t.AssignDate).ToListAsync(cancellationToken);

        return new ReportDataDto
        {
            Title = "تقرير التوصيل",
            TotalCount = rows.Count,
            Rows = rows.Select(t => new Dictionary<string, object?>
            {
                ["الطلب"] = t.Order.OrderNumber,
                ["العميل"] = t.Order.Customer.FullName,
                ["السائق"] = t.Driver.DriverName,
                ["تاريخ التعيين"] = t.AssignDate.ToString("yyyy-MM-dd"),
                ["الحالة"] = t.Status.ToString()
            }).ToList()
        };
    }

    public async Task<byte[]> ExportOrdersExcelAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var data = filter.ReportType switch
        {
            "revenue" => await GetRevenueReportAsync(filter, cancellationToken),
            "customers" => await GetCustomersReportAsync(filter, cancellationToken),
            "drivers" => await GetDriversReportAsync(filter, cancellationToken),
            "delivery" => await GetDeliveryReportAsync(filter, cancellationToken),
            _ => await GetOrdersReportAsync(filter, cancellationToken)
        };

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(data.Title);
        if (data.Rows.Count == 0) return Array.Empty<byte>();

        var headers = data.Rows[0].Keys.ToList();
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        for (var r = 0; r < data.Rows.Count; r++)
            for (var c = 0; c < headers.Count; c++)
                ws.Cell(r + 2, c + 1).Value = data.Rows[r][headers[c]]?.ToString() ?? "";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportOrdersPdfAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var data = await GetOrdersReportAsync(filter, cancellationToken);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    col.Item().Text(data.Title).Bold().FontSize(16);
                    col.Item().Text($"العدد: {data.TotalCount} | الإجمالي: {data.TotalAmount:N2}");
                    foreach (var row in data.Rows.Take(100))
                    {
                        col.Item().Text(string.Join(" | ", row.Select(kv => $"{kv.Key}: {kv.Value}")));
                    }
                });
            });
        });
        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}
