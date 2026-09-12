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

    public async Task<IReadOnlyList<string>> GetRoleNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserRoles.Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && ur.Role!.IsActive == true)
            .Select(r => r.Role!.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasRoleByUserIdAndRoleNameName(
        long userId,
        string roleName,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Role> query =
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Name == roleName && r.IsActive
            select r;

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission?>> GetPermissionsByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Permission> query =
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            join p in _dbContext.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && r.IsActive == true && p.IsActive == true
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
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            join p in _dbContext.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && r.IsActive == true && p.IsActive == true
            select p.Name;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionByUserIdAndPermissionName(
        long userId,
        string permissionName,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Permission> query =
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            join p in _dbContext.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && p.Name == permissionName && r.IsActive && p.IsActive
            select p;

        return await query.AnyAsync(cancellationToken);
    }
}
