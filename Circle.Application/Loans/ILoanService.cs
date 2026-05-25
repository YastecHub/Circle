using Circle.Domain.Enums;

namespace Circle.Application.Loans;

public interface ILoanService
{
    Task<LoanListResponse> GetLoansAsync(
        string? search,
        LoanStatus? status,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<LoanResponse> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default);

    Task<LoanStatementResponse> GetLoanStatementAsync(Guid loanId, CancellationToken cancellationToken = default);

    Task<LoanResponse> CreateLoanAsync(CreateLoanRequest request, CancellationToken cancellationToken = default);

    Task<LoanResponse> ApproveLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default);

    Task<LoanResponse> RejectLoanAsync(Guid loanId, RejectLoanRequest request, CancellationToken cancellationToken = default);
}
