using Circle.Domain.Enums;

namespace Circle.Application.Customers;

public sealed record CreateCustomerRequest(
    string? CustomerCode,
    string? PassbookReference,
    string FullName,
    string Phone,
    string? AlternatePhone,
    string Address,
    string? City,
    string? State,
    string? GovernmentIdType,
    string? GovernmentIdNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Occupation,
    string? GuarantorName,
    string? GuarantorPhone,
    string? GuarantorRelationship,
    string? GuarantorIdNumber,
    Guid? AssignedCreditOfficerId,
    string? Notes);

public sealed record UpdateCustomerRequest(
    string FullName,
    string Phone,
    string? AlternatePhone,
    string Address,
    string? City,
    string? State,
    string? GovernmentIdType,
    string? GovernmentIdNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Occupation,
    string? GuarantorName,
    string? GuarantorPhone,
    string? GuarantorRelationship,
    string? GuarantorIdNumber,
    Guid? AssignedCreditOfficerId,
    string? Notes);

public sealed record CustomerResponse(
    Guid Id,
    string CustomerCode,
    string PassbookReference,
    string FullName,
    string Phone,
    string? AlternatePhone,
    string Address,
    string? City,
    string? State,
    string? GovernmentIdType,
    string? GovernmentIdNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Occupation,
    string? GuarantorName,
    string? GuarantorPhone,
    string? GuarantorRelationship,
    string? GuarantorIdNumber,
    Guid AssignedCreditOfficerId,
    CustomerStatus Status,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CustomerStatementResponse(
    CustomerResponse Customer,
    CustomerStatementSummary Summary,
    CustomerStatementVirtualAccount? ActiveVirtualAccount,
    IReadOnlyCollection<CustomerStatementLoanLine> Loans,
    IReadOnlyCollection<CustomerStatementRepaymentLine> RecentRepayments,
    IReadOnlyCollection<CustomerStatementCollectionLine> RecentCollectionActivities,
    IReadOnlyCollection<CustomerStatementDocumentLine> Documents);

public sealed record CustomerStatementSummary(
    int LoanCount,
    int ActiveLoanCount,
    decimal TotalPrincipal,
    decimal TotalRepayable,
    decimal TotalDisbursed,
    decimal TotalPaid,
    decimal OutstandingBalance,
    decimal OverdueBalance,
    DateOnly? NextDueDate,
    decimal? NextDueAmount);

public sealed record CustomerStatementVirtualAccount(
    Guid Id,
    string Provider,
    string AccountName,
    string AccountNumber,
    string BankName,
    string? BankCode,
    string? ProviderReference,
    VirtualAccountStatus Status);

public sealed record CustomerStatementLoanLine(
    Guid Id,
    string LoanCode,
    LoanStatus Status,
    decimal PrincipalAmount,
    decimal TotalRepayable,
    decimal TotalPaid,
    decimal OutstandingBalance,
    decimal OverdueBalance,
    DateOnly StartDate,
    DateOnly MaturityDate,
    DateOnly? NextDueDate,
    decimal? NextDueAmount);

public sealed record CustomerStatementRepaymentLine(
    Guid Id,
    Guid LoanId,
    decimal Amount,
    DateOnly PaymentDate,
    RepaymentMethod Method,
    string? Reference,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record CustomerStatementCollectionLine(
    Guid Id,
    Guid? LoanId,
    Guid? RepaymentScheduleId,
    Guid OfficerUserId,
    CollectionActivityType ActivityType,
    CollectionOutcome Outcome,
    DateOnly? PromisedPaymentDate,
    decimal? PromisedAmount,
    string Notes,
    DateTime CreatedAtUtc);

public sealed record CustomerStatementDocumentLine(
    Guid Id,
    DocumentType DocumentType,
    string FileName,
    string? ContentType,
    Guid UploadedByUserId,
    DateTime CreatedAtUtc);

public sealed record CustomerListResponse(
    IReadOnlyCollection<CustomerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
