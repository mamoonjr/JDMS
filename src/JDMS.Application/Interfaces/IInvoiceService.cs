namespace JDMS.Application.Interfaces;

public interface IInvoiceService
{
    Task<byte[]> GeneratePdfAsync(int orderId, bool thermal = false, CancellationToken cancellationToken = default);
    Task<int> CreateInvoiceForOrderAsync(int orderId, CancellationToken cancellationToken = default);
}
