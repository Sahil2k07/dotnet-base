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

    public async Task<IReadOnlyList<Permission?>> GetPermissionsByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Permission> query =
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _dbContext.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<string> query =
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _dbContext.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p.Name;

        return await query.Distinct().ToListAsync(cancellationToken);
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
