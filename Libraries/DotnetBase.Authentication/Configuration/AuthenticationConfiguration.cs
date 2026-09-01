namespace DotnetBase.Authentication.Configuration;

public sealed class AuthenticationOption
{
    public required string JwtSigningSecret { get; set; }

    public required string JwtEncryptionSecret { get; set; }

    public required string JwtIssuer { get; set; }

    public required string JwtAudience { get; set; }

    public int AccessTokenExpirationMinutes { get; set; }

    public int RefreshTokenExpirationDays { get; set; }

    public int BcryptWorkFactor { get; set; }
}
