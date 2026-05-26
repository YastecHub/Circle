namespace Circle.Application.Search;

public interface IGlobalSearchService
{
    Task<GlobalSearchResponse> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken = default);
}
