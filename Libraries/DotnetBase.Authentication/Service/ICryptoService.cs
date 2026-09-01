using DotnetBase.Authentication.Claims;

namespace DotnetBase.Authentication.Service;

public interface ICryptoService
{
    Task<string> GenerateAccessToken(AccessTokenClaims accessTokenClaims);

    Task<string> GenerateRefreshToken(RefreshTokenClaims refreshTokenClaims);

    Task<AccessTokenClaims> GetAccessTokenClaims(string accessToken);

    Task<RefreshTokenClaims> GetRefreshTokenClaims(string refreshToken);

    string GeneratePasswordHash(string password);

    bool VerifyPassword(string password, string passwordHash);
}
