using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class CustomerDocument : BaseEntity
{
    private CustomerDocument()
    {
    }

    public CustomerDocument(
        Guid customerId,
        DocumentType documentType,
        string fileName,
        string storagePath,
        string? contentType,
        Guid uploadedByUserId)
    {
        CustomerId = customerId;
        DocumentType = documentType;
        FileName = fileName.Trim();
        StoragePath = storagePath.Trim();
        ContentType = contentType?.Trim();
        UploadedByUserId = uploadedByUserId;
    }

    public Guid CustomerId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public Guid UploadedByUserId { get; private set; }
}
