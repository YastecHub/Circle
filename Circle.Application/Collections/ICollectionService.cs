namespace Circle.Application.Collections;

public interface ICollectionService
{
    Task<CollectionActivityListResponse> GetActivitiesAsync(
        Guid? customerId,
        Guid? loanId,
        Guid? officerUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CollectionActivityResponse> RecordActivityAsync(RecordCollectionActivityRequest request, CancellationToken cancellationToken = default);

    Task<RefreshOverdueSchedulesResponse> RefreshOverdueSchedulesAsync(CancellationToken cancellationToken = default);
}
