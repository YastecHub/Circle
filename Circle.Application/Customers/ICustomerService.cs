using Circle.Domain.Enums;

namespace Circle.Application.Customers;

public interface ICustomerService
{
    Task<CustomerListResponse> GetCustomersAsync(
        string? search,
        CustomerStatus? status,
        Guid? assignedCreditOfficerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerStatementResponse> GetCustomerStatementAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerResponse> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerResponse> ReassignCustomerAsync(Guid customerId, ReassignCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerResponse> ArchiveCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
