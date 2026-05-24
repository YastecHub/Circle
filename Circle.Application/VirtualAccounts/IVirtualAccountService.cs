using Circle.Domain.Enums;

namespace Circle.Application.VirtualAccounts;

public interface IVirtualAccountService
{
    Task<CustomerVirtualAccountListResponse> GetVirtualAccountsAsync(
        string? search,
        Guid? customerId,
        VirtualAccountStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CustomerVirtualAccountResponse> GetCustomerVirtualAccountAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerVirtualAccountResponse> RegisterCustomerVirtualAccountAsync(Guid customerId, RegisterVirtualAccountRequest request, CancellationToken cancellationToken = default);

    Task<CustomerVirtualAccountResponse> DeactivateCustomerVirtualAccountAsync(Guid customerId, CancellationToken cancellationToken = default);
}
