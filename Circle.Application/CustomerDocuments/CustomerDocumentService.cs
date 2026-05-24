using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.CustomerDocuments;

public sealed class CustomerDocumentService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService) : ICustomerDocumentService
{
    private const long MaximumFileSizeBytes = 10 * 1024 * 1024;

    public async Task<IReadOnlyCollection<CustomerDocumentResponse>> GetCustomerDocumentsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);

        return await dbContext.CustomerDocuments
            .AsNoTracking()
            .Where(document => document.CustomerId == customerId)
            .OrderByDescending(document => document.CreatedAtUtc)
            .Select(document => ToResponse(document))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDocumentResponse> UploadCustomerDocumentAsync(Guid customerId, UploadCustomerDocumentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUploadRequest(request);

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);

        var storagePath = await fileStorageService.SaveCustomerDocumentAsync(customer.Id, request.FileName, request.Content, cancellationToken);
        var document = new CustomerDocument(
            customer.Id,
            request.DocumentType,
            request.FileName,
            storagePath,
            request.ContentType,
            currentUser.Id);

        dbContext.CustomerDocuments.Add(document);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "UploadCustomerDocument", $"Customer:{customer.CustomerCode}", null, request.FileName, currentUserService.IpAddress, request.DocumentType.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(document);
    }

    public async Task<CustomerDocumentFileResponse> DownloadCustomerDocumentAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);

        var document = await dbContext.CustomerDocuments
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == documentId && candidate.CustomerId == customerId, cancellationToken)
            ?? throw new AppException("Customer document was not found.");

        try
        {
            var content = await fileStorageService.OpenReadAsync(document.StoragePath, cancellationToken);
            return new CustomerDocumentFileResponse(
                document.FileName,
                string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
                content);
        }
        catch (FileNotFoundException)
        {
            throw new AppException("Stored customer document file was not found.");
        }
    }

    public async Task DeleteCustomerDocumentAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);
        var document = await dbContext.CustomerDocuments
            .SingleOrDefaultAsync(candidate => candidate.Id == documentId && candidate.CustomerId == customerId, cancellationToken)
            ?? throw new AppException("Customer document was not found.");

        dbContext.CustomerDocuments.Remove(document);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "DeleteCustomerDocument", $"Customer:{customer.CustomerCode}", document.FileName, null, currentUserService.IpAddress, document.DocumentType.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        await fileStorageService.DeleteAsync(document.StoragePath, cancellationToken);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");
    }

    private async Task<Customer> EnsureCanAccessCustomerAsync(Guid customerId, User currentUser, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        if (currentUser.Role == UserRole.CreditOfficer && customer.AssignedCreditOfficerId != currentUser.Id)
        {
            throw new AppException("Credit officers can only manage documents for assigned customers.");
        }

        return customer;
    }

    private static void ValidateUploadRequest(UploadCustomerDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new AppException("Document file name is required.");
        }

        if (request.Content.Length <= 0)
        {
            throw new AppException("Document file is empty.");
        }

        if (request.Content.Length > MaximumFileSizeBytes)
        {
            throw new AppException("Document file cannot exceed 10 MB.");
        }
    }

    private static CustomerDocumentResponse ToResponse(CustomerDocument document)
    {
        return new CustomerDocumentResponse(
            document.Id,
            document.CustomerId,
            document.DocumentType,
            document.FileName,
            document.StoragePath,
            document.ContentType,
            document.UploadedByUserId,
            document.CreatedAtUtc);
    }
}
