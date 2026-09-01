namespace DotnetBase.Authentication.Claims;

public sealed class AccessTokenClaims
{
    public long UserId { get; set; }

    public long UserProfileId { get; set; }

    public required string ActiveRole { get; set; }

    public required IReadOnlyList<string> Roles { get; set; }

    public required IReadOnlyList<string> Permissions { get; set; }
}
