namespace Circle.Application.Disbursements;

public interface IDisbursementService
{
    Task<DisbursementListResponse> GetDisbursementsAsync(
        Guid? loanId,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<DisbursementResponse> RecordDisbursementAsync(RecordDisbursementRequest request, CancellationToken cancellationToken = default);
}
