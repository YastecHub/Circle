using Circle.Domain.Enums;
using System.Text.Json;

namespace Circle.Application.Webhooks;

public interface IPaymentWebhookService
{
    Task<PaymentWebhookResponse> ReceiveMonnifyWebhookAsync(
        JsonElement payload,
        bool isVerified,
        CancellationToken cancellationToken = default);

    Task<PaymentWebhookListResponse> GetWebhookEventsAsync(
        PaymentWebhookStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
