using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface IPosService
{
    Task<CustomerLookupDto?> LookupByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<PosSubmitResult> SubmitAsync(PosSubmitModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PosProductDto>> SearchProductsAsync(string? query, int limit = 40, CancellationToken cancellationToken = default);
    Task<PosStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<decimal?> GetAreaDeliveryFeeAsync(int areaId, CancellationToken cancellationToken = default);
}
