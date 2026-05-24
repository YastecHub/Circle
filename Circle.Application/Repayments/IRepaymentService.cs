namespace Circle.Application.Repayments;

public interface IRepaymentService
{
    Task<RepaymentListResponse> GetRepaymentsAsync(
        Guid? loanId,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RepaymentResponse> RecordRepaymentAsync(RecordRepaymentRequest request, CancellationToken cancellationToken = default);
}
