namespace DotnetBase.Contract.Auth.Claims;

public sealed class AccessTokenClaims
{
    public long UserId { get; set; }

    public long UserProfileId { get; set; }

    public DateTime ExpiresAt { get; set; }
}
