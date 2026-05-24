using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class CustomerVirtualAccount : BaseEntity
{
    private CustomerVirtualAccount()
    {
    }

    public CustomerVirtualAccount(
        Guid customerId,
        string provider,
        string accountName,
        string accountNumber,
        string bankName,
        string? bankCode,
        string? providerReference,
        Guid createdByUserId)
    {
        CustomerId = customerId;
        Provider = provider.Trim();
        AccountName = accountName.Trim();
        AccountNumber = accountNumber.Trim();
        BankName = bankName.Trim();
        BankCode = string.IsNullOrWhiteSpace(bankCode) ? null : bankCode.Trim();
        ProviderReference = string.IsNullOrWhiteSpace(providerReference) ? null : providerReference.Trim();
        CreatedByUserId = createdByUserId;
        Status = VirtualAccountStatus.Active;
    }

    public Guid CustomerId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string BankName { get; private set; } = string.Empty;
    public string? BankCode { get; private set; }
    public string? ProviderReference { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public VirtualAccountStatus Status { get; private set; }

    public void Deactivate()
    {
        Status = VirtualAccountStatus.Inactive;
    }
}
