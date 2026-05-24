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

public sealed record CustomerListResponse(
    IReadOnlyCollection<CustomerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
