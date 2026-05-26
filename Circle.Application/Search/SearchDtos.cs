namespace Circle.Application.Search;

public sealed record GlobalSearchResponse(
    string Query,
    IReadOnlyCollection<GlobalSearchResultResponse> Results);

public sealed record GlobalSearchResultResponse(
    string ResultType,
    Guid Id,
    string Title,
    string? Subtitle,
    string? MatchedField,
    Guid? CustomerId,
    string? CustomerName,
    Guid? LoanId,
    string? LoanCode,
    decimal? Amount,
    string? Status,
    DateTime CreatedAtUtc);
