using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Data.Repository.Implementation;

public sealed class RoleRepository : IRoleRepository
{
    private readonly DotnetBaseContext _dbContext;

    public RoleRepository(DotnetBaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetRoleByRoleName(
        string role,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Roles.Where(r => r.Name == role && r.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission?>> GetRolePermissionsByRoleId(
        long roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .RolePermissions.Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionNamesByRoleId(
        long roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .RolePermissions.Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission!.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserRoles.Include(r => r.Role)
            .Where(r => r.UserId == userId)
            .Select(r => r.Role!.Name)
            .ToListAsync(cancellationToken);
    }
}
