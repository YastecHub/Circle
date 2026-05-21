using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class Customer : BaseEntity
{
    private readonly List<CustomerDocument> _documents = [];

    private Customer()
    {
    }

    public Customer(
        string customerCode,
        string passbookReference,
        string fullName,
        string phone,
        string address,
        Guid assignedCreditOfficerId)
    {
        CustomerCode = customerCode.Trim();
        PassbookReference = passbookReference.Trim();
        FullName = fullName.Trim();
        Phone = phone.Trim();
        Address = address.Trim();
        AssignedCreditOfficerId = assignedCreditOfficerId;
        Status = CustomerStatus.Active;
    }

    public string CustomerCode { get; private set; } = string.Empty;
    public string PassbookReference { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? AlternatePhone { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? GovernmentIdType { get; private set; }
    public string? GovernmentIdNumber { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? Occupation { get; private set; }
    public string? GuarantorName { get; private set; }
    public string? GuarantorPhone { get; private set; }
    public string? GuarantorRelationship { get; private set; }
    public string? GuarantorIdNumber { get; private set; }
    public Guid AssignedCreditOfficerId { get; private set; }
    public CustomerStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<CustomerDocument> Documents => _documents.AsReadOnly();

    public void UpdateProfile(
        string fullName,
        string phone,
        string? alternatePhone,
        string address,
        string? city,
        string? state,
        string? occupation,
        string? notes)
    {
        FullName = fullName.Trim();
        Phone = phone.Trim();
        AlternatePhone = string.IsNullOrWhiteSpace(alternatePhone) ? null : alternatePhone.Trim();
        Address = address.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
        Occupation = string.IsNullOrWhiteSpace(occupation) ? null : occupation.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SetKyc(
        string? governmentIdType,
        string? governmentIdNumber,
        DateOnly? dateOfBirth,
        string? gender)
    {
        GovernmentIdType = string.IsNullOrWhiteSpace(governmentIdType) ? null : governmentIdType.Trim();
        GovernmentIdNumber = string.IsNullOrWhiteSpace(governmentIdNumber) ? null : governmentIdNumber.Trim();
        DateOfBirth = dateOfBirth;
        Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
    }

    public void SetGuarantor(
        string? name,
        string? phone,
        string? relationship,
        string? idNumber)
    {
        GuarantorName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        GuarantorPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        GuarantorRelationship = string.IsNullOrWhiteSpace(relationship) ? null : relationship.Trim();
        GuarantorIdNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber.Trim();
    }

    public void AssignCreditOfficer(Guid creditOfficerId)
    {
        AssignedCreditOfficerId = creditOfficerId;
    }

    public void Archive()
    {
        Status = CustomerStatus.Archived;
    }
}
