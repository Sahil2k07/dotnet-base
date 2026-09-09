namespace DotnetBase.Contract.Auth.Claims;

public sealed class RefreshTokenClaims
{
    public Guid SessionId { get; set; }

    public long UserRoleId { get; set; }

    public DateTime ExpiresAt { get; set; }
}
