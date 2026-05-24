namespace Circle.Application.Administration;

public sealed record SystemSettingResponse(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record UpdateSystemSettingRequest(string Value, string? Reason);

public sealed record AuditLogResponse(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string AffectedRecord,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? Reason,
    DateTime CreatedAtUtc);

public sealed record AuditLogListResponse(
    IReadOnlyCollection<AuditLogResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
