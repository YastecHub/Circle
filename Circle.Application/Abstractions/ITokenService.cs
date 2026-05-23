using Circle.Domain.Entities;

namespace Circle.Application.Abstractions;

public interface ITokenService
{
    string CreateAccessToken(User user, DateTime expiresAtUtc);
    string CreateRefreshToken();
}
