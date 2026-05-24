using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class Repayment : BaseEntity
{
    private Repayment()
    {
    }

    public Repayment(
        Guid loanId,
        Guid customerId,
        decimal amount,
        DateOnly paymentDate,
        RepaymentMethod method,
        string? reference,
        Guid recordedByUserId,
        string? notes)
    {
        LoanId = loanId;
        CustomerId = customerId;
        Amount = amount;
        PaymentDate = paymentDate;
        Method = method;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        RecordedByUserId = recordedByUserId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid LoanId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public RepaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public string? Notes { get; private set; }
}
