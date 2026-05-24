namespace Circle.Application.Administration;

public interface IAdminService
{
    Task<IReadOnlyCollection<SystemSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<SystemSettingResponse> UpdateSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken cancellationToken = default);

    Task<AuditLogListResponse> GetAuditLogsAsync(
        string? action,
        string? affectedRecord,
        Guid? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
