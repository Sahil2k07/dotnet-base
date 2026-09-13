using DotnetBase.Authentication.Configuration;
using DotnetBase.Authentication.Service.Implementation;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Shared.Exceptions;
using Microsoft.Extensions.Options;

namespace DotnetBase.Server.UnitTest.Authentication;

public sealed class CryptoServiceTests
{
    private readonly CryptoService _cryptoService;

    public CryptoServiceTests()
    {
        var options = Options.Create(
            new AuthenticationOption
            {
                JwtSigningSecret = "this-is-a-test-signing-secret-at-least-32-chars",
                JwtEncryptionSecret = "12345678901234567890123456789012",
                JwtIssuer = "dotnet-base",
                JwtAudience = "dotnet-base-client",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7,
                BcryptWorkFactor = 4,
            }
        );

        _cryptoService = new CryptoService(options);
    }

    [Fact]
    public async Task GenerateAccessToken_WithValidClaims_ReturnsTokenAndExpiration()
    {
        var claims = new AccessTokenClaims { UserId = 123, UserProfileId = 456 };

        var (token, expiresAt) = await _cryptoService.GenerateAccessToken(claims);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateAccessToken_WithValidClaims_CanReadClaimsBack()
    {
        var claims = new AccessTokenClaims { UserId = 123, UserProfileId = 456 };

        var (token, _) = await _cryptoService.GenerateAccessToken(claims);

        var result = await _cryptoService.GetAccessTokenClaims(token);

        Assert.Equal(123, result.UserId);
        Assert.Equal(456, result.UserProfileId);
    }

    [Fact]
    public async Task GetAccessTokenClaims_WithInvalidToken_ThrowsAuthenticationException()
    {
        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _cryptoService.GetAccessTokenClaims("invalid-token")
        );

        Assert.Equal("malformed token", exception.Message);
    }

    [Fact]
    public async Task GenerateRefreshToken_WithValidClaims_ReturnsTokenAndExpiration()
    {
        var sessionId = Guid.NewGuid();

        var claims = new RefreshTokenClaims { SessionId = sessionId };

        var (token, expiresAt) = await _cryptoService.GenerateRefreshToken(claims);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateRefreshToken_WithValidClaims_CanReadClaimsBack()
    {
        var sessionId = Guid.NewGuid();

        var claims = new RefreshTokenClaims { SessionId = sessionId };

        var (token, _) = await _cryptoService.GenerateRefreshToken(claims);

        var result = await _cryptoService.GetRefreshTokenClaims(token);

        Assert.Equal(sessionId, result.SessionId);
    }

    [Fact]
    public async Task GetRefreshTokenClaims_WithInvalidToken_ThrowsAuthenticationException()
    {
        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _cryptoService.GetRefreshTokenClaims("invalid-token")
        );

        Assert.Equal("malformed token", exception.Message);
    }

    [Fact]
    public void GeneratePasswordHash_WithValidPassword_ReturnsHash()
    {
        var password = "Password@123";

        var hash = _cryptoService.GeneratePasswordHash(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var password = "Password@123";

        var hash = _cryptoService.GeneratePasswordHash(password);

        var result = _cryptoService.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        var password = "Password@123";

        var hash = _cryptoService.GeneratePasswordHash(password);

        var result = _cryptoService.VerifyPassword("WrongPassword@123", hash);

        Assert.False(result);
    }

    [Fact]
    public void HashRefreshToken_WithValidToken_ReturnsHash()
    {
        var refreshToken = "refresh-token";

        var hash = _cryptoService.HashRefreshToken(refreshToken);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(refreshToken, hash);
    }

    [Fact]
    public void HashRefreshToken_WithEmptyToken_ThrowsAuthenticationException()
    {
        var exception = Assert.Throws<AuthenticationException>(() =>
            _cryptoService.HashRefreshToken("")
        );

        Assert.Equal("Refresh token is required.", exception.Message);
    }

    [Fact]
    public void VerifyRefreshTokenHash_WithCorrectToken_ReturnsTrue()
    {
        var refreshToken = "refresh-token";

        var hash = _cryptoService.HashRefreshToken(refreshToken);

        var result = _cryptoService.VerifyRefreshTokenHash(refreshToken, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyRefreshTokenHash_WithIncorrectToken_ReturnsFalse()
    {
        var refreshToken = "refresh-token";

        var hash = _cryptoService.HashRefreshToken(refreshToken);

        var result = _cryptoService.VerifyRefreshTokenHash("different-refresh-token", hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyRefreshTokenHash_WithEmptyToken_ReturnsFalse()
    {
        var hash = _cryptoService.HashRefreshToken("refresh-token");

        var result = _cryptoService.VerifyRefreshTokenHash("", hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyRefreshTokenHash_WithEmptyStoredHash_ReturnsFalse()
    {
        var result = _cryptoService.VerifyRefreshTokenHash("refresh-token", "");

        Assert.False(result);
    }
}
