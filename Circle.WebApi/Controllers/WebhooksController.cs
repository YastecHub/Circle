using Circle.Application.Webhooks;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Circle.WebApi.Controllers;

[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController(
    IPaymentWebhookService paymentWebhookService,
    IConfiguration configuration) : ControllerBase
{
    private const string MonnifySecretHeaderName = "X-Circle-Webhook-Secret";

    [AllowAnonymous]
    [HttpPost("monnify")]
    public async Task<IActionResult> ReceiveMonnifyWebhook([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var secret = configuration["Webhooks:MonnifySecret"];
        if (!string.IsNullOrWhiteSpace(secret) && !HasValidSecret(secret))
        {
            return Unauthorized(new { error = "Invalid webhook secret." });
        }

        var response = await paymentWebhookService.ReceiveMonnifyWebhookAsync(
            payload,
            !string.IsNullOrWhiteSpace(secret),
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("monnify/events")]
    public async Task<IActionResult> GetMonnifyWebhookEvents(
        [FromQuery] PaymentWebhookStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await paymentWebhookService.GetWebhookEventsAsync(status, page, pageSize, cancellationToken));
    }

    private bool HasValidSecret(string expectedSecret)
    {
        if (!Request.Headers.TryGetValue(MonnifySecretHeaderName, out var submittedSecret) ||
            string.IsNullOrWhiteSpace(submittedSecret))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedSecret);
        var submittedBytes = Encoding.UTF8.GetBytes(submittedSecret.ToString());

        return expectedBytes.Length == submittedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, submittedBytes);
    }
}
