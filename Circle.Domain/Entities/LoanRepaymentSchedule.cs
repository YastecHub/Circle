using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class LoanRepaymentSchedule : BaseEntity
{
    private LoanRepaymentSchedule()
    {
    }

    public LoanRepaymentSchedule(
        Guid loanId,
        int installmentNumber,
        DateOnly dueDate,
        decimal principalAmount,
        decimal interestAmount,
        decimal totalAmount)
    {
        LoanId = loanId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        PrincipalAmount = principalAmount;
        InterestAmount = interestAmount;
        TotalAmount = totalAmount;
        Status = RepaymentScheduleStatus.Pending;
    }

    public Guid LoanId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal AmountPaid { get; private set; }
    public RepaymentScheduleStatus Status { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    public void MarkPaid(decimal amountPaid, DateTime paidAtUtc)
    {
        AmountPaid = amountPaid;
        PaidAtUtc = paidAtUtc;
        Status = amountPaid >= TotalAmount ? RepaymentScheduleStatus.Paid : RepaymentScheduleStatus.Pending;
    }
}
