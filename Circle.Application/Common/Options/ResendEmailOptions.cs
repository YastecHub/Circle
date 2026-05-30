namespace Circle.Application.Common.Options;

public sealed class ResendEmailOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "onboarding@resend.dev";
    public string FromName { get; init; } = "Circle";
}
