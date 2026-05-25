using Circle.Application.Abstractions;
using Circle.Application.Reconciliation;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Circle.Application.Webhooks;

public sealed class PaymentWebhookService(
    IApplicationDbContext dbContext,
    IReconciliationService reconciliationService,
    IDateTimeProvider dateTimeProvider) : IPaymentWebhookService
{
    private const int MaximumPageSize = 100;
    private const string MonnifyProvider = "Monnify";

    public async Task<PaymentWebhookResponse> ReceiveMonnifyWebhookAsync(
        JsonElement payload,
        bool isVerified,
        CancellationToken cancellationToken = default)
    {
        var eventType = ExtractString(payload, EventTypePaths);
        var eventReference = ExtractString(payload, ReferencePaths);
        var webhookEvent = new PaymentWebhookEvent(
            MonnifyProvider,
            eventType,
            eventReference,
            payload.GetRawText(),
            isVerified);

        if (!string.IsNullOrWhiteSpace(eventReference) &&
            await dbContext.PaymentWebhookEvents.AnyAsync(candidate =>
                    candidate.Provider == MonnifyProvider &&
                    candidate.EventReference == eventReference &&
                    candidate.Status != PaymentWebhookStatus.Failed,
                cancellationToken))
        {
            webhookEvent.MarkDuplicate("A webhook event with this provider reference has already been received.");
            dbContext.PaymentWebhookEvents.Add(webhookEvent);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(webhookEvent, null);
        }

        dbContext.PaymentWebhookEvents.Add(webhookEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (IsClearlyUnsuccessful(payload, eventType))
        {
            webhookEvent.MarkIgnored("Webhook event does not represent a successful payment.");
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(webhookEvent, null);
        }

        if (!TryBuildInboundPaymentRequest(payload, eventReference, out var request, out var failureReason))
        {
            webhookEvent.MarkFailed(failureReason);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(webhookEvent, null);
        }

        try
        {
            var inboundPayment = await reconciliationService.RecordSystemInboundPaymentAsync(
                request,
                "MonnifyWebhook",
                cancellationToken);

            webhookEvent.MarkProcessed(inboundPayment.Id, $"Inbound payment status: {inboundPayment.Status}.");
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(webhookEvent, inboundPayment);
        }
        catch (Exception exception)
        {
            webhookEvent.MarkFailed(exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(webhookEvent, null);
        }
    }

    public async Task<PaymentWebhookListResponse> GetWebhookEventsAsync(
        PaymentWebhookStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = dbContext.PaymentWebhookEvents.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(webhookEvent => webhookEvent.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(webhookEvent => webhookEvent.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = events.Select(webhookEvent => ToResponse(webhookEvent, null)).ToList();

        return new PaymentWebhookListResponse(items, page, pageSize, totalCount);
    }

    private bool TryBuildInboundPaymentRequest(
        JsonElement payload,
        string? eventReference,
        out RecordInboundPaymentRequest request,
        out string failureReason)
    {
        request = default!;
        var accountNumber = ExtractString(payload, AccountNumberPaths);
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            failureReason = "Webhook payload does not include a destination virtual account number.";
            return false;
        }

        var amount = ExtractDecimal(payload, AmountPaths);
        if (amount is null or <= 0)
        {
            failureReason = "Webhook payload does not include a valid payment amount.";
            return false;
        }

        var paidAtUtc = ExtractDateTime(payload, PaidAtPaths) ?? dateTimeProvider.UtcNow;
        request = new RecordInboundPaymentRequest(
            eventReference,
            accountNumber,
            amount.Value,
            paidAtUtc,
            ExtractString(payload, PayerNamePaths),
            ExtractString(payload, NarrationPaths));

        failureReason = string.Empty;
        return true;
    }

    private static bool IsClearlyUnsuccessful(JsonElement payload, string? eventType)
    {
        var status = ExtractString(payload, PaymentStatusPaths);
        var combined = $"{eventType} {status}".Trim();
        if (string.IsNullOrWhiteSpace(combined))
        {
            return false;
        }

        return ContainsAny(combined, "failed", "failure", "pending", "abandoned", "cancelled", "canceled", "reversed");
    }

    private static bool ContainsAny(string value, params string[] fragments)
    {
        return fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractString(JsonElement root, IReadOnlyCollection<string[]> paths)
    {
        foreach (var path in paths)
        {
            if (TryGetPath(root, path, out var value))
            {
                if (value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }

                if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                {
                    return value.ToString();
                }
            }
        }

        return null;
    }

    private static decimal? ExtractDecimal(JsonElement root, IReadOnlyCollection<string[]> paths)
    {
        foreach (var path in paths)
        {
            if (!TryGetPath(root, path, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String &&
                decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static DateTime? ExtractDateTime(JsonElement root, IReadOnlyCollection<string[]> paths)
    {
        foreach (var path in paths)
        {
            if (!TryGetPath(root, path, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                return parsed.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                    : parsed.ToUniversalTime();
            }
        }

        return null;
    }

    private static bool TryGetPath(JsonElement root, IReadOnlyList<string> path, out JsonElement value)
    {
        value = root;
        foreach (var segment in path)
        {
            if (value.ValueKind != JsonValueKind.Object ||
                !TryGetProperty(value, segment, out value))
            {
                value = default;
                return false;
            }
        }

        return true;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals(name) || string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static PaymentWebhookResponse ToResponse(PaymentWebhookEvent webhookEvent, InboundPaymentResponse? inboundPayment)
    {
        return new PaymentWebhookResponse(
            webhookEvent.Id,
            webhookEvent.Provider,
            webhookEvent.EventType,
            webhookEvent.EventReference,
            webhookEvent.IsVerified,
            webhookEvent.Status,
            webhookEvent.InboundPaymentId,
            webhookEvent.ProcessingMessage,
            inboundPayment,
            webhookEvent.CreatedAtUtc);
    }

    private static readonly string[][] EventTypePaths =
    [
        ["eventType"],
        ["event"],
        ["type"],
        ["eventData", "eventType"],
        ["data", "eventType"]
    ];

    private static readonly string[][] ReferencePaths =
    [
        ["eventReference"],
        ["transactionReference"],
        ["paymentReference"],
        ["reference"],
        ["eventData", "transactionReference"],
        ["eventData", "paymentReference"],
        ["eventData", "reference"],
        ["data", "transactionReference"],
        ["data", "paymentReference"],
        ["data", "reference"]
    ];

    private static readonly string[][] AccountNumberPaths =
    [
        ["accountNumber"],
        ["destinationAccountNumber"],
        ["eventData", "accountNumber"],
        ["eventData", "destinationAccountNumber"],
        ["eventData", "accountDetails", "accountNumber"],
        ["data", "accountNumber"],
        ["data", "destinationAccountNumber"],
        ["data", "accountDetails", "accountNumber"]
    ];

    private static readonly string[][] AmountPaths =
    [
        ["amount"],
        ["amountPaid"],
        ["eventData", "amount"],
        ["eventData", "amountPaid"],
        ["eventData", "accountDetails", "amountPaid"],
        ["data", "amount"],
        ["data", "amountPaid"],
        ["data", "accountDetails", "amountPaid"]
    ];

    private static readonly string[][] PaidAtPaths =
    [
        ["paidAtUtc"],
        ["paidAt"],
        ["paidOn"],
        ["paymentDate"],
        ["eventData", "paidAtUtc"],
        ["eventData", "paidAt"],
        ["eventData", "paidOn"],
        ["eventData", "paymentDate"],
        ["data", "paidAtUtc"],
        ["data", "paidAt"],
        ["data", "paidOn"],
        ["data", "paymentDate"]
    ];

    private static readonly string[][] PayerNamePaths =
    [
        ["payerName"],
        ["customerName"],
        ["eventData", "payerName"],
        ["eventData", "customerName"],
        ["eventData", "customer", "name"],
        ["eventData", "accountDetails", "accountName"],
        ["data", "payerName"],
        ["data", "customerName"],
        ["data", "customer", "name"],
        ["data", "accountDetails", "accountName"]
    ];

    private static readonly string[][] NarrationPaths =
    [
        ["narration"],
        ["paymentDescription"],
        ["eventData", "narration"],
        ["eventData", "paymentDescription"],
        ["data", "narration"],
        ["data", "paymentDescription"]
    ];

    private static readonly string[][] PaymentStatusPaths =
    [
        ["paymentStatus"],
        ["status"],
        ["eventData", "paymentStatus"],
        ["eventData", "status"],
        ["data", "paymentStatus"],
        ["data", "status"]
    ];
}
