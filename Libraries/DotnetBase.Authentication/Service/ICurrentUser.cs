using DotnetBase.Contract.Auth.Claims;

namespace DotnetBase.Authentication.Service;

public interface ICurrentUser
{
    long UserId { get; }

    long UserProfileId { get; }

    bool IsAuthenticated { get; }

    AccessTokenClaims GetAccessTokenClaims();

    Task<IReadOnlyList<string>> GetRoles();

    Task<bool> HasRole(string role);

    Task<IReadOnlyList<string>> GetPermissions();

    Task<bool> HasPermission(string permission);
}
