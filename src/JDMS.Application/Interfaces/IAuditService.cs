using JDMS.Domain.Enums;

namespace JDMS.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditActionType actionType, string entityName, string? entityId = null,
        string? oldValues = null, string? newValues = null, CancellationToken cancellationToken = default);
}
