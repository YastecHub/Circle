namespace Circle.Application.Common.Options;

public sealed class OtpOptions
{
    public const string SectionName = "Otp";

    public int CodeLength { get; init; } = 6;
    public int ExpiryMinutes { get; init; } = 10;
    public bool IncludeCodeInResponse { get; init; }
}
