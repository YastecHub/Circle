namespace Circle.Domain.Enums;

public enum InboundPaymentStatus
{
    Received = 1,
    Applied = 2,
    PartiallyApplied = 3,
    Unmatched = 4,
    Duplicate = 5,
    Failed = 6
}
