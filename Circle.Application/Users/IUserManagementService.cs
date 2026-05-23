namespace Circle.Application.Users;

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> InviteCreditOfficerAsync(InviteCreditOfficerRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> SuspendUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> SetPasswordAsync(Guid userId, SetUserPasswordRequest request, CancellationToken cancellationToken = default);
}
