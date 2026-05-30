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
        var recipientEmail = string.IsNullOrWhiteSpace(emailOptions.OverrideToEmail)
            ? user.Email
            : emailOptions.OverrideToEmail.Trim();
        var isOverriddenRecipient = !string.Equals(recipientEmail, user.Email, StringComparison.OrdinalIgnoreCase);
        var safeName = WebUtility.HtmlEncode(user.FullName);
        var safeCode = WebUtility.HtmlEncode(code);
        var safeRecipientNote = isOverriddenRecipient
            ? $"<p style=\"color: #65758a;\">This OTP was requested for <strong>{WebUtility.HtmlEncode(user.Email)}</strong>.</p>"
            : string.Empty;

        var message = new EmailMessage
        {
            From = FormatSender(emailOptions),
            Subject = "Your Circle login OTP",
            TextBody = BuildTextBody(user, code, isOverriddenRecipient),
            HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; color: #17212b; line-height: 1.5;">
                  <p>Hello {safeName},</p>
                  <p>Your Circle login OTP is:</p>
                  <p style="font-size: 28px; font-weight: 700; letter-spacing: 4px; margin: 18px 0;">{safeCode}</p>
                  {safeRecipientNote}
                  <p>This code expires shortly. If you did not request this login, please ignore this email.</p>
                </div>
                """
        };
        message.To.Add(recipientEmail);

        await resend.EmailSendAsync(message, cancellationToken);
        logger.LogInformation(
            "Sent Circle OTP email using Resend. LoginEmail={LoginEmail}; RecipientEmail={RecipientEmail}; RecipientOverridden={RecipientOverridden}",
            user.Email,
            recipientEmail,
            isOverriddenRecipient);
    }

    private static string FormatSender(ResendEmailOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            return options.FromEmail;
        }

        return $"{options.FromName} <{options.FromEmail}>";
    }

    private static string BuildTextBody(User user, string code, bool isOverriddenRecipient)
    {
        var text = $"Hello {user.FullName}, your Circle login OTP is {code}. It expires shortly. If you did not request this, please ignore this email.";

        return isOverriddenRecipient
            ? $"{text} This OTP was requested for {user.Email}."
            : text;
    }
}
