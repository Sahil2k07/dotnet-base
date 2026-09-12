using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace DotnetBase.Authentication.Authorization.Handler;

public sealed class RoleAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    private readonly IRoleRepository _roleRepository;

    public RoleAuthorizationHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RolesAuthorizationRequirement requirement
    )
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        string? userIdValue = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userIdValue, out long userId))
        {
            return;
        }

        foreach (string role in requirement.AllowedRoles)
        {
            bool hasRole = await _roleRepository.HasRoleByUserIdAndRoleNameName(userId, role);

            if (hasRole)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
