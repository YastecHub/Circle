using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class LoanDisbursement : BaseEntity
{
    private LoanDisbursement()
    {
    }

    public LoanDisbursement(
        Guid loanId,
        Guid customerId,
        decimal amount,
        DateOnly disbursementDate,
        DisbursementMethod method,
        string? reference,
        Guid disbursedByUserId,
        string? notes)
    {
        LoanId = loanId;
        CustomerId = customerId;
        Amount = amount;
        DisbursementDate = disbursementDate;
        Method = method;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        DisbursedByUserId = disbursedByUserId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid LoanId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly DisbursementDate { get; private set; }
    public DisbursementMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public Guid DisbursedByUserId { get; private set; }
    public string? Notes { get; private set; }
}
