using Circle.Domain.Enums;

namespace Circle.Application.VirtualAccounts;

public sealed record RegisterVirtualAccountRequest(
    string AccountName,
    string AccountNumber,
    string BankName,
    string? BankCode,
    string? ProviderReference);

public sealed record CustomerVirtualAccountResponse(
    Guid Id,
    Guid CustomerId,
    string Provider,
    string AccountName,
    string AccountNumber,
    string BankName,
    string? BankCode,
    string? ProviderReference,
    Guid CreatedByUserId,
    VirtualAccountStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CustomerVirtualAccountListResponse(
    IReadOnlyCollection<CustomerVirtualAccountResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
