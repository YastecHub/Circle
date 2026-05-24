using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class InboundPayment : BaseEntity
{
    private InboundPayment()
    {
    }

    public InboundPayment(
        string provider,
        string? providerReference,
        string accountNumber,
        decimal amount,
        DateTime paidAtUtc,
        string? payerName,
        string? narration,
        Guid? recordedByUserId)
    {
        Provider = provider.Trim();
        ProviderReference = string.IsNullOrWhiteSpace(providerReference) ? null : providerReference.Trim();
        AccountNumber = accountNumber.Trim();
        Amount = amount;
        PaidAtUtc = paidAtUtc;
        PayerName = string.IsNullOrWhiteSpace(payerName) ? null : payerName.Trim();
        Narration = string.IsNullOrWhiteSpace(narration) ? null : narration.Trim();
        RecordedByUserId = recordedByUserId;
        Status = InboundPaymentStatus.Received;
        UnappliedAmount = amount;
    }

    public string Provider { get; private set; } = string.Empty;
    public string? ProviderReference { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal AppliedAmount { get; private set; }
    public decimal UnappliedAmount { get; private set; }
    public DateTime PaidAtUtc { get; private set; }
    public string? PayerName { get; private set; }
    public string? Narration { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? LoanId { get; private set; }
    public Guid? RepaymentId { get; private set; }
    public Guid? RecordedByUserId { get; private set; }
    public InboundPaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    public void MarkDuplicate(string reason)
    {
        Status = InboundPaymentStatus.Duplicate;
        FailureReason = reason.Trim();
    }

    public void MarkUnmatched(string reason)
    {
        Status = InboundPaymentStatus.Unmatched;
        FailureReason = reason.Trim();
    }

    public void MarkFailed(string reason)
    {
        Status = InboundPaymentStatus.Failed;
        FailureReason = reason.Trim();
    }

    public void Apply(Guid customerId, Guid loanId, Guid repaymentId, decimal appliedAmount)
    {
        CustomerId = customerId;
        LoanId = loanId;
        RepaymentId = repaymentId;
        AppliedAmount = appliedAmount;
        UnappliedAmount = Amount - appliedAmount;
        FailureReason = UnappliedAmount > 0 ? "Payment exceeded outstanding loan balance." : null;
        Status = UnappliedAmount > 0 ? InboundPaymentStatus.PartiallyApplied : InboundPaymentStatus.Applied;
    }
}
