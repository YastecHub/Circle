using Circle.Application.Abstractions;
using Circle.Application.Common.Constants;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Administration;

public sealed class AdminService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IAdminService
{
    private const int MaximumPageSize = 100;

    public async Task<IReadOnlyCollection<SystemSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SystemSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Key)
            .Select(setting => ToSettingResponse(setting))
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemSettingResponse> UpdateSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            throw new AppException("Setting value is required.");
        }

        var setting = await dbContext.SystemSettings.SingleOrDefaultAsync(candidate => candidate.Key == key, cancellationToken)
            ?? throw new AppException("System setting was not found.");

        var normalizedValue = NormalizeSettingValue(setting.Key, request.Value);
        var oldValue = setting.Value;

        if (oldValue == normalizedValue)
        {
            return ToSettingResponse(setting);
        }

        setting.UpdateValue(normalizedValue);
        dbContext.AuditLogs.Add(new AuditLog(
            currentUserService.UserId,
            "UpdateSystemSetting",
            $"SystemSetting:{setting.Key}",
            oldValue,
            normalizedValue,
            currentUserService.IpAddress,
            request.Reason));

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToSettingResponse(setting);
    }

    public async Task<AuditLogListResponse> GetAuditLogsAsync(
        string? action,
        string? affectedRecord,
        Guid? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim();
            query = query.Where(log => log.Action.Contains(normalizedAction));
        }

        if (!string.IsNullOrWhiteSpace(affectedRecord))
        {
            var normalizedAffectedRecord = affectedRecord.Trim();
            query = query.Where(log => log.AffectedRecord.Contains(normalizedAffectedRecord));
        }

        if (actorUserId is not null)
        {
            query = query.Where(log => log.ActorUserId == actorUserId);
        }

        if (fromUtc is not null)
        {
            query = query.Where(log => log.CreatedAtUtc >= fromUtc);
        }

        if (toUtc is not null)
        {
            query = query.Where(log => log.CreatedAtUtc <= toUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(log => log.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => ToAuditLogResponse(log))
            .ToListAsync(cancellationToken);

        return new AuditLogListResponse(items, page, pageSize, totalCount);
    }

    private static string NormalizeSettingValue(string key, string value)
    {
        var trimmedValue = value.Trim();

        return key switch
        {
            SystemSettingKeys.PaymentGateway => NormalizePaymentGateway(trimmedValue),
            SystemSettingKeys.InterestCalculationMethod => NormalizeInterestCalculationMethod(trimmedValue),
            SystemSettingKeys.AutoApprovalThreshold => NormalizeAutoApprovalThreshold(trimmedValue),
            SystemSettingKeys.RepaymentFrequencies => NormalizeRepaymentFrequencies(trimmedValue),
            _ => trimmedValue
        };
    }

    private static string NormalizePaymentGateway(string value)
    {
        if (!string.Equals(value, "Monnify", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("Only Monnify is supported as the MVP payment gateway.");
        }

        return "Monnify";
    }

    private static string NormalizeInterestCalculationMethod(string value)
    {
        if (!Enum.TryParse<InterestCalculationMethod>(value, ignoreCase: true, out var method) ||
            method != InterestCalculationMethod.FlatRate)
        {
            throw new AppException("Only FlatRate interest calculation is supported for the MVP.");
        }

        return method.ToString();
    }

    private static string NormalizeAutoApprovalThreshold(string value)
    {
        if (!decimal.TryParse(value, out var threshold) || threshold < 0)
        {
            throw new AppException("Auto approval threshold must be a non-negative amount.");
        }

        return threshold.ToString("0.##");
    }

    private static string NormalizeRepaymentFrequencies(string value)
    {
        var frequencies = value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(candidate =>
            {
                if (!Enum.TryParse<RepaymentFrequency>(candidate, ignoreCase: true, out var frequency))
                {
                    throw new AppException("Repayment frequencies must be Daily, Weekly, or both.");
                }

                return frequency;
            })
            .Distinct()
            .OrderBy(frequency => frequency)
            .ToList();

        if (frequencies.Count == 0)
        {
            throw new AppException("At least one repayment frequency is required.");
        }

        return string.Join(',', frequencies.Select(frequency => frequency.ToString()));
    }

    private static SystemSettingResponse ToSettingResponse(SystemSetting setting)
    {
        return new SystemSettingResponse(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.Description,
            setting.CreatedAtUtc,
            setting.UpdatedAtUtc);
    }

    private static AuditLogResponse ToAuditLogResponse(AuditLog log)
    {
        return new AuditLogResponse(
            log.Id,
            log.ActorUserId,
            log.Action,
            log.AffectedRecord,
            log.OldValue,
            log.NewValue,
            log.IpAddress,
            log.Reason,
            log.CreatedAtUtc);
    }
}
