using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Http;

namespace DotnetBase.Authentication.Service.Implementation;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IRoleRepository _roleRepository;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, IRoleRepository roleRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _roleRepository = roleRepository;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public long UserId => long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public long UserProfileId => long.Parse(User.FindFirstValue("user_profile_id")!);

    public DateTime ExpiresAt =>
        User.FindFirstValue(JwtRegisteredClaimNames.Exp) is string exp
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime
            : DateTime.MinValue;

    public AccessTokenClaims GetAccessTokenClaims()
    {
        return new AccessTokenClaims
        {
            UserId = UserId,
            UserProfileId = UserProfileId,
            ExpiresAt = ExpiresAt,
        };
    }

    public async Task<IReadOnlyList<string>> GetRoles()
    {
        return await _roleRepository.GetRoleNamesByUserId(UserId);
    }

    public async Task<bool> HasRole(string role)
    {
        return await _roleRepository.HasRoleByUserIdAndRoleNameName(UserId, role);
    }

    public async Task<IReadOnlyList<string>> GetPermissions()
    {
        return await _roleRepository.GetPermissionNamesByUserId(UserId);
    }

    public async Task<bool> HasPermission(string permission)
    {
        return await _roleRepository.HasRoleByUserIdAndRoleNameName(UserId, permission);
    }
}
