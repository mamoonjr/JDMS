namespace JDMS.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalOrders { get; set; }
    public int NewOrders { get; set; }
    public int OrdersInProgress { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalDrivers { get; set; }
}

public class DashboardChartsDto
{
    public List<ChartPointDto> DailyOrders { get; set; } = new();
    public List<ChartPointDto> MonthlyRevenue { get; set; } = new();
    public List<ChartPointDto> OrdersByGovernorate { get; set; } = new();
    public List<ChartPointDto> OrdersByStatus { get; set; } = new();
}

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class DashboardRecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
}
