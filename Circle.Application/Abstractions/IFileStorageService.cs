namespace Circle.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveCustomerDocumentAsync(Guid customerId, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
