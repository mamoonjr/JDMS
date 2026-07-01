using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderListDto>> GetFilteredAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, OrderCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, int status, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    string GenerateOrderNumber();
}
