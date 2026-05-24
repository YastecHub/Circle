using Circle.Domain.Enums;

namespace Circle.Application.CustomerDocuments;

public sealed record UploadCustomerDocumentRequest(
    DocumentType DocumentType,
    string FileName,
    string? ContentType,
    Stream Content);

public sealed record CustomerDocumentResponse(
    Guid Id,
    Guid CustomerId,
    DocumentType DocumentType,
    string FileName,
    string StoragePath,
    string? ContentType,
    Guid UploadedByUserId,
    DateTime CreatedAtUtc);

public sealed record CustomerDocumentFileResponse(
    string FileName,
    string ContentType,
    Stream Content);
