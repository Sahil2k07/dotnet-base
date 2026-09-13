using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using DotnetBase.Shared.Constant;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Server.IntegrationTest.Infrastructure.Seed;

public static class RolePermissionSeed
{
    private static readonly (string Role, string Permission)[] Entries =
    [
        (Roles.SUPER_ADMIN, Permissions.READ_ROLE),
        (Roles.SUPER_ADMIN, Permissions.CREATE_ROLE),
        (Roles.SUPER_ADMIN, Permissions.UPDATE_ROLE),
        (Roles.SUPER_ADMIN, Permissions.DELETE_ROLE),
        (Roles.SUPER_ADMIN, Permissions.READ_PERMISSION),
        (Roles.ADMIN, Permissions.READ_ROLE),
        (Roles.ADMIN, Permissions.CREATE_ROLE),
        (Roles.ADMIN, Permissions.UPDATE_ROLE),
        (Roles.ADMIN, Permissions.DELETE_ROLE),
        (Roles.ADMIN, Permissions.READ_PERMISSION),
    ];

    public static async Task SeedAsync(
        DotnetBaseContext context,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entry in Entries)
        {
            Role? role = await context.Roles.SingleAsync(
                r => r.Name == entry.Role,
                cancellationToken
            );

            Permission? permission = await context.Permissions.SingleAsync(
                p => p.Name == entry.Permission,
                cancellationToken
            );

            bool exists = await context.RolePermissions.AnyAsync(
                rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id,
                cancellationToken
            );

            if (exists)
                continue;

            context.RolePermissions.Add(
                new RolePermission { RoleId = role.Id, PermissionId = permission.Id }
            );
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
