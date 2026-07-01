using JDMS.Application.DTOs;

namespace JDMS.Application.Interfaces;

public interface IQuickEntryService
{
    Task<CustomerLookupDto?> LookupByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<QuickEntrySubmitResult> SubmitAsync(QuickEntryViewModel model, CancellationToken cancellationToken = default);
}
