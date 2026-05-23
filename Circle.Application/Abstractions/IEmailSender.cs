using Circle.Domain.Entities;

namespace Circle.Application.Abstractions;

public interface IEmailSender
{
    Task SendOtpAsync(User user, string code, CancellationToken cancellationToken = default);
}
