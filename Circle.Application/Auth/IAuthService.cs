namespace Circle.Application.Auth;

public interface IAuthService
{
    Task<AuthUserResponse> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthTokenResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
}
