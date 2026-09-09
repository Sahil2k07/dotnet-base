using DotnetBase.Contract.Auth.Claims;

namespace DotnetBase.Authentication.Service;

public interface ICurrentUser
{
    long UserId { get; }

    long UserProfileId { get; }

    IReadOnlyList<string> Roles { get; }

    IReadOnlyList<string> Permissions { get; }

    bool IsAuthenticated { get; }

    bool HasPermission(string permission);

    bool HasRole(string role);

    AccessTokenClaims GetAccessTokenClaims();
}
