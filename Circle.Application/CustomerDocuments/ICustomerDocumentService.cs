namespace Circle.Application.CustomerDocuments;

public interface ICustomerDocumentService
{
    Task<IReadOnlyCollection<CustomerDocumentResponse>> GetCustomerDocumentsAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerDocumentResponse> UploadCustomerDocumentAsync(Guid customerId, UploadCustomerDocumentRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDocumentFileResponse> DownloadCustomerDocumentAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);

    Task DeleteCustomerDocumentAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
}
