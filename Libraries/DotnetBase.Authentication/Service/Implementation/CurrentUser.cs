using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DotnetBase.Authentication.Service.Implementation;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public long UserId => long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public long UserProfileId => long.Parse(User.FindFirstValue("user_profile_id")!);

    public string ActiveRole => User.FindFirstValue("active_role")!;

    public IReadOnlyList<string> Roles => [.. User.FindAll("roles").Select(claim => claim.Value)];

    public IReadOnlyList<string> Permissions =>
        [.. User.FindAll("permissions").Select(claim => claim.Value)];

    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
