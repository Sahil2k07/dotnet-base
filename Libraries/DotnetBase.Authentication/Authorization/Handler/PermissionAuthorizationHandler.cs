using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Authentication.Authorization.Requirement;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Authorization;

namespace DotnetBase.Authentication.Authorization.Handler;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleRepository _roleRepository;

    public PermissionAuthorizationHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
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

        bool hasPermission = await _roleRepository.HasPermissionByUserIdAndPermissionName(
            userId,
            requirement.Permission
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
