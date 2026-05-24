using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class Loan : BaseEntity
{
    private readonly List<LoanRepaymentSchedule> _repaymentSchedules = [];

    private Loan()
    {
    }

    public Loan(
        string loanCode,
        Guid customerId,
        decimal principalAmount,
        decimal interestRatePercent,
        decimal interestAmount,
        decimal totalRepayable,
        int termCount,
        RepaymentFrequency repaymentFrequency,
        InterestCalculationMethod interestCalculationMethod,
        DateOnly startDate,
        DateOnly maturityDate,
        Guid requestedByUserId,
        string? purpose)
    {
        LoanCode = loanCode.Trim();
        CustomerId = customerId;
        PrincipalAmount = principalAmount;
        InterestRatePercent = interestRatePercent;
        InterestAmount = interestAmount;
        TotalRepayable = totalRepayable;
        TermCount = termCount;
        RepaymentFrequency = repaymentFrequency;
        InterestCalculationMethod = interestCalculationMethod;
        StartDate = startDate;
        MaturityDate = maturityDate;
        RequestedByUserId = requestedByUserId;
        Purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose.Trim();
        Status = LoanStatus.PendingOwnerApproval;
    }

    public string LoanCode { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestRatePercent { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal TotalRepayable { get; private set; }
    public int TermCount { get; private set; }
    public RepaymentFrequency RepaymentFrequency { get; private set; }
    public InterestCalculationMethod InterestCalculationMethod { get; private set; }
    public LoanStatus Status { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly MaturityDate { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public string? ApprovalNote { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Purpose { get; private set; }
    public IReadOnlyCollection<LoanRepaymentSchedule> RepaymentSchedules => _repaymentSchedules.AsReadOnly();

    public void Approve(Guid approvedByUserId, DateTime approvedAtUtc, string? approvalNote)
    {
        Status = LoanStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = approvedAtUtc;
        ApprovalNote = string.IsNullOrWhiteSpace(approvalNote) ? null : approvalNote.Trim();
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        Status = LoanStatus.Rejected;
        RejectionReason = reason.Trim();
    }

    public void MarkDisbursed()
    {
        Status = LoanStatus.Disbursed;
    }

    public void MarkClosed()
    {
        Status = LoanStatus.Closed;
    }

    public void AddRepaymentSchedule(LoanRepaymentSchedule repaymentSchedule)
    {
        _repaymentSchedules.Add(repaymentSchedule);
    }
}
