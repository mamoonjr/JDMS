using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var orders = _unitOfWork.Orders.Query();
        var delivered = await orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var revenue = await orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.GrandTotal, cancellationToken);

        return new DashboardStatsDto
        {
            TotalOrders = await orders.CountAsync(cancellationToken),
            NewOrders = await orders.CountAsync(o => o.Status == OrderStatus.New, cancellationToken),
            OrdersInProgress = await orders.CountAsync(o =>
                o.Status == OrderStatus.Processing ||
                o.Status == OrderStatus.ReadyForDelivery ||
                o.Status == OrderStatus.OutForDelivery, cancellationToken),
            DeliveredOrders = delivered,
            CancelledOrders = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken),
            TotalRevenue = revenue,
            TotalCustomers = await _unitOfWork.Customers.Query().CountAsync(cancellationToken),
            TotalDrivers = await _unitOfWork.Drivers.Query().CountAsync(d => d.IsActive, cancellationToken)
        };
    }

    public async Task<DashboardChartsDto> GetChartsAsync(CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.AddDays(-30);
        var ordersQuery = _unitOfWork.Orders.Query()
            .Include(o => o.Address).ThenInclude(a => a.Governorate)
            .Where(o => o.OrderDate >= from);

        var dailyRaw = await ordersQuery
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var daily = dailyRaw.Select(x => new ChartPointDto
        {
            Label = x.Date.ToString("yyyy-MM-dd"),
            Value = x.Count
        }).ToList();

        var monthlyRaw = await _unitOfWork.Orders.Query()
            .Where(o => o.Status == OrderStatus.Delivered && o.OrderDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.GrandTotal) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var monthly = monthlyRaw.Select(x => new ChartPointDto
        {
            Label = $"{x.Year}-{x.Month:D2}",
            Value = x.Total
        }).ToList();

        var byGovRaw = await ordersQuery
            .GroupBy(o => o.Address.Governorate.NameAr)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var byGov = byGovRaw.Select(x => new ChartPointDto
        {
            Label = x.Name,
            Value = x.Count
        }).ToList();

        var statusNames = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.New, "جديد" },
            { OrderStatus.Processing, "قيد المعالجة" },
            { OrderStatus.ReadyForDelivery, "جاهز للتوصيل" },
            { OrderStatus.OutForDelivery, "قيد التوصيل" },
            { OrderStatus.Delivered, "تم التسليم" },
            { OrderStatus.Cancelled, "ملغي" },
            { OrderStatus.Returned, "مرتجع" }
        };

        var byStatus = await _unitOfWork.Orders.Query()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new DashboardChartsDto
        {
            DailyOrders = daily,
            MonthlyRevenue = monthly,
            OrdersByGovernorate = byGov,
            OrdersByStatus = byStatus.Select(s => new ChartPointDto
            {
                Label = statusNames.GetValueOrDefault(s.Status, s.Status.ToString()),
                Value = s.Count
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var statusNames = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.New, "جديد" },
            { OrderStatus.Processing, "قيد المعالجة" },
            { OrderStatus.ReadyForDelivery, "جاهز للتوصيل" },
            { OrderStatus.OutForDelivery, "قيد التوصيل" },
            { OrderStatus.Delivered, "تم التسليم" },
            { OrderStatus.Cancelled, "ملغي" },
            { OrderStatus.Returned, "مرتجع" }
        };

        var rows = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                CustomerName = o.Customer.FullName,
                o.OrderDate,
                Status = o.Status,
                o.GrandTotal,
                o.Notes
            })
            .ToListAsync(cancellationToken);

        return rows.Select(o => new DashboardRecentOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            OrderDate = o.OrderDate,
            StatusName = statusNames.GetValueOrDefault(o.Status, o.Status.ToString()),
            GrandTotal = o.GrandTotal,
            Notes = o.Notes
        }).ToList();
    }
}
