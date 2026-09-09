using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetBase.Authentication.Configuration;
using DotnetBase.Contract.Auth.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotnetBase.Authentication.Service.Implementation;

public sealed class CryptoService : ICryptoService
{
    private readonly AuthenticationOption _authenticationOptions;

    public CryptoService(IOptions<AuthenticationOption> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public Task<(string, DateTime)> GenerateAccessToken(AccessTokenClaims accessTokenClaims)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, accessTokenClaims.UserId.ToString()),
            new("user_profile_id", accessTokenClaims.UserProfileId.ToString()),
            new("active_role", accessTokenClaims.ActiveRole),
        };

        claims.AddRange(accessTokenClaims.Roles.Select(role => new Claim("roles", role)));

        claims.AddRange(
            accessTokenClaims.Permissions.Select(permission => new Claim("permissions", permission))
        );

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtSigningSecret)
        );

        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var encryptionKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtEncryptionSecret)
        );

        var encryptionCredentials = new EncryptingCredentials(
            encryptionKey,
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes256CbcHmacSha512
        );

        var identity = new ClaimsIdentity(claims);

        var tokenHandler = new JwtSecurityTokenHandler();

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(
            _authenticationOptions.AccessTokenExpirationMinutes
        );

        var token = tokenHandler.CreateEncodedJwt(
            _authenticationOptions.JwtIssuer,
            _authenticationOptions.JwtAudience,
            identity,
            null,
            expiresAt,
            DateTime.UtcNow,
            signingCredentials,
            encryptionCredentials
        );

        return Task.FromResult((token, expiresAt));
    }

    public Task<(string, DateTime)> GenerateRefreshToken(RefreshTokenClaims refreshTokenClaims)
    {
        var claims = new List<Claim>
        {
            new("session_id", refreshTokenClaims.SessionId.ToString()),
            new("user_role_id", refreshTokenClaims.UserRoleId.ToString()),
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtSigningSecret)
        );

        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var encryptionKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtEncryptionSecret)
        );

        var encryptionCredentials = new EncryptingCredentials(
            encryptionKey,
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes256CbcHmacSha512
        );

        var identity = new ClaimsIdentity(claims);

        var tokenHandler = new JwtSecurityTokenHandler();

        DateTime expiresAt = DateTime.UtcNow.AddDays(
            _authenticationOptions.RefreshTokenExpirationDays
        );

        var token = tokenHandler.CreateEncodedJwt(
            _authenticationOptions.JwtIssuer,
            _authenticationOptions.JwtAudience,
            identity,
            null,
            expiresAt,
            DateTime.UtcNow,
            signingCredentials,
            encryptionCredentials
        );

        return Task.FromResult((token, expiresAt));
    }

    public Task<AccessTokenClaims> GetAccessTokenClaims(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtSigningSecret)
        );

        var encryptionKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtEncryptionSecret)
        );

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            TokenDecryptionKey = encryptionKey,

            ValidateIssuer = true,
            ValidIssuer = _authenticationOptions.JwtIssuer,

            ValidateAudience = true,
            ValidAudience = _authenticationOptions.JwtAudience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,
        };

        var principal = tokenHandler.ValidateToken(
            accessToken,
            validationParameters,
            out var validatedToken
        );

        var userId = long.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        var userProfileId = long.Parse(principal.FindFirstValue("user_profile_id")!);

        var activeRole = principal.FindFirstValue("active_role")!;

        var roles = principal.FindAll("roles").Select(x => x.Value).ToList();

        var permissions = principal.FindAll("permissions").Select(x => x.Value).ToList();

        var claims = new AccessTokenClaims
        {
            UserId = userId,
            UserProfileId = userProfileId,
            ActiveRole = activeRole,
            Roles = roles,
            Permissions = permissions,
            ExpiresAt = validatedToken.ValidTo,
        };

        return Task.FromResult(claims);
    }

    public Task<RefreshTokenClaims> GetRefreshTokenClaims(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtSigningSecret)
        );

        var encryptionKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authenticationOptions.JwtEncryptionSecret)
        );

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            TokenDecryptionKey = encryptionKey,

            ValidateIssuer = true,
            ValidIssuer = _authenticationOptions.JwtIssuer,

            ValidateAudience = true,
            ValidAudience = _authenticationOptions.JwtAudience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,
        };

        var principal = tokenHandler.ValidateToken(
            refreshToken,
            validationParameters,
            out var validatedToken
        );

        var sessionId = Guid.Parse(principal.FindFirstValue("session_id")!);

        var userRoleId = long.Parse(principal.FindFirstValue("user_role_id")!);

        var claims = new RefreshTokenClaims
        {
            SessionId = sessionId,
            UserRoleId = userRoleId,
            ExpiresAt = validatedToken.ValidTo,
        };

        return Task.FromResult(claims);
    }

    public string GeneratePasswordHash(string password)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword(
            password,
            _authenticationOptions.BcryptWorkFactor
        );

        return hash;
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AuthenticationException("Refresh token is required.");
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }

    public bool VerifyRefreshTokenHash(string refreshToken, string storedTokenHash)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(storedTokenHash))
        {
            return false;
        }

        var incomingTokenHash = HashRefreshToken(refreshToken);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(incomingTokenHash),
            Convert.FromHexString(storedTokenHash)
        );
    }
}
