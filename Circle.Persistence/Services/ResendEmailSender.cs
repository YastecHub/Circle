using System.Net;
using Circle.Application.Abstractions;
using Circle.Application.Common.Options;
using Circle.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Circle.Persistence.Services;

public sealed class ResendEmailSender(
    IResend resend,
    IOptions<ResendEmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task SendOtpAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value;
        var safeName = WebUtility.HtmlEncode(user.FullName);
        var safeCode = WebUtility.HtmlEncode(code);

        var message = new EmailMessage
        {
            From = FormatSender(emailOptions),
            Subject = "Your Circle login OTP",
            TextBody = $"Hello {user.FullName}, your Circle login OTP is {code}. It expires shortly. If you did not request this, please ignore this email.",
            HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; color: #17212b; line-height: 1.5;">
                  <p>Hello {safeName},</p>
                  <p>Your Circle login OTP is:</p>
                  <p style="font-size: 28px; font-weight: 700; letter-spacing: 4px; margin: 18px 0;">{safeCode}</p>
                  <p>This code expires shortly. If you did not request this login, please ignore this email.</p>
                </div>
                """
        };
        message.To.Add(user.Email);

        await resend.EmailSendAsync(message, cancellationToken);
        logger.LogInformation("Sent Circle OTP email to {Email} using Resend.", user.Email);
    }

    private static string FormatSender(ResendEmailOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            return options.FromEmail;
        }

        return $"{options.FromName} <{options.FromEmail}>";
    }
}
