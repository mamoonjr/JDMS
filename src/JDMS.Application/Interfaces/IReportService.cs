using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> ExportOrdersExcelAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrdersPdfAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportDataDto> GetOrdersReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportDataDto> GetRevenueReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportDataDto> GetCustomersReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportDataDto> GetDriversReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportDataDto> GetDeliveryReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
}
