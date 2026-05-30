using Circle.Application.CustomerDocuments;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer,CreditOfficer")]
[Route("api/customers/{customerId:guid}/documents")]
public sealed class CustomerDocumentsController(ICustomerDocumentService customerDocumentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomerDocuments(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerDocumentService.GetCustomerDocumentsAsync(customerId, cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCustomerDocument(
        Guid customerId,
        [FromForm] DocumentType documentType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "Document file is required." });
        }

        await using var stream = file.OpenReadStream();
        var response = await customerDocumentService.UploadCustomerDocumentAsync(
            customerId,
            new UploadCustomerDocumentRequest(documentType, file.FileName, file.ContentType, stream),
            cancellationToken);

        return CreatedAtAction(nameof(GetCustomerDocuments), new { customerId }, response);
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> DownloadCustomerDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var response = await customerDocumentService.DownloadCustomerDocumentAsync(customerId, documentId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> DeleteCustomerDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        await customerDocumentService.DeleteCustomerDocumentAsync(customerId, documentId, cancellationToken);
        return NoContent();
    }
}


