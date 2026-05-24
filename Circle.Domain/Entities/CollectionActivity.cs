using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class CollectionActivity : BaseEntity
{
    private CollectionActivity()
    {
    }

    public CollectionActivity(
        Guid customerId,
        Guid? loanId,
        Guid? repaymentScheduleId,
        Guid officerUserId,
        CollectionActivityType activityType,
        CollectionOutcome outcome,
        DateOnly? promisedPaymentDate,
        decimal? promisedAmount,
        string notes)
    {
        CustomerId = customerId;
        LoanId = loanId;
        RepaymentScheduleId = repaymentScheduleId;
        OfficerUserId = officerUserId;
        ActivityType = activityType;
        Outcome = outcome;
        PromisedPaymentDate = promisedPaymentDate;
        PromisedAmount = promisedAmount;
        Notes = notes.Trim();
    }

    public Guid CustomerId { get; private set; }
    public Guid? LoanId { get; private set; }
    public Guid? RepaymentScheduleId { get; private set; }
    public Guid OfficerUserId { get; private set; }
    public CollectionActivityType ActivityType { get; private set; }
    public CollectionOutcome Outcome { get; private set; }
    public DateOnly? PromisedPaymentDate { get; private set; }
    public decimal? PromisedAmount { get; private set; }
    public string Notes { get; private set; } = string.Empty;
}
