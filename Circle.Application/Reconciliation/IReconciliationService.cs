using Circle.Domain.Enums;

namespace Circle.Application.Reconciliation;

public interface IReconciliationService
{
    Task<InboundPaymentListResponse> GetInboundPaymentsAsync(
        string? search,
        InboundPaymentStatus? status,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<InboundPaymentResponse> RecordInboundPaymentAsync(RecordInboundPaymentRequest request, CancellationToken cancellationToken = default);
}
