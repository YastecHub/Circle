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
    public decimal OutstandingAmount => TotalAmount - AmountPaid;
    public RepaymentScheduleStatus Status { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    public decimal ApplyPayment(decimal amount, DateTime paidAtUtc)
    {
        if (amount <= 0 || Status == RepaymentScheduleStatus.Paid)
        {
            return amount;
        }

        var amountToApply = Math.Min(amount, OutstandingAmount);
        AmountPaid += amountToApply;

        if (AmountPaid >= TotalAmount)
        {
            AmountPaid = TotalAmount;
            PaidAtUtc = paidAtUtc;
            Status = RepaymentScheduleStatus.Paid;
        }

        return amount - amountToApply;
    }

    public bool MarkOverdue(DateOnly today)
    {
        if (Status != RepaymentScheduleStatus.Pending || DueDate >= today)
        {
            return false;
        }

        Status = RepaymentScheduleStatus.Overdue;
        return true;
    }
}
