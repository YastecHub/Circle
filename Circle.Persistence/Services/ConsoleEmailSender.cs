using Circle.Application.Abstractions;
using Circle.Domain.Entities;

namespace Circle.Persistence.Services;

public sealed class ConsoleEmailSender : IEmailSender
{
    public Task SendOtpAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Circle OTP for {0}: {1}", user.Email, code);
        return Task.CompletedTask;
    }
}
