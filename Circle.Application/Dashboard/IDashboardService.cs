namespace Circle.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
}
