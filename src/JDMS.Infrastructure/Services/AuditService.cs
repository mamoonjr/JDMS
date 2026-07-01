using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace JDMS.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AuditActionType actionType, string entityName, string? entityId = null,
        string? oldValues = null, string? newValues = null, CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var log = new AuditLog
        {
            UserId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            UserName = user?.Identity?.Name,
            ActionType = actionType,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            ActionDate = DateTime.UtcNow
        };
        await _unitOfWork.AuditLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
