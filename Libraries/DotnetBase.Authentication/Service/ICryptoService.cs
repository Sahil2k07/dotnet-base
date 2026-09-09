using DotnetBase.Contract.Auth.Claims;

namespace DotnetBase.Authentication.Service;

public interface ICryptoService
{
    Task<(string accessToken, DateTime expiresAt)> GenerateAccessToken(
        AccessTokenClaims accessTokenClaims
    );

    Task<(string refreshToken, DateTime expiresAt)> GenerateRefreshToken(
        RefreshTokenClaims refreshTokenClaims
    );

    Task<AccessTokenClaims> GetAccessTokenClaims(string accessToken);

    Task<RefreshTokenClaims> GetRefreshTokenClaims(string refreshToken);

    string GeneratePasswordHash(string password);

    bool VerifyPassword(string password, string passwordHash);

    string HashRefreshToken(string refreshToken);

    bool VerifyRefreshTokenHash(string refreshToken, string storedTokenHash);
}
