namespace Circle.WebApi.Contracts;

public sealed record HealthResponse(
    string Status,
    string Service,
    DateTime CheckedAtUtc);

public sealed record DatabaseHealthResponse(
    string Status,
    string Database);
