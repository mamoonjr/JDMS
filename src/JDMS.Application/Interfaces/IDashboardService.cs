using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<DashboardChartsDto> GetChartsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default);
}
