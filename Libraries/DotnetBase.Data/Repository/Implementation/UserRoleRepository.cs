using System.Diagnostics;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using DotnetBase.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Data.Repository.Implementation;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly DotnetBaseContext _dbContext;

    private readonly IRoleRepository _roleRepository;

    public UserRoleRepository(DotnetBaseContext dbContext, IRoleRepository roleRepository)
    {
        _dbContext = dbContext;
        _roleRepository = roleRepository;
    }

    public async Task<UserRole?> GetUserRole(
        long userRoleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserRoles.Include(ur => ur.Role)
            .Where(ur => ur.Id == userRoleId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserRole> AddUserRole(
        long userId,
        string roleName,
        CancellationToken cancellationToken = default
    )
    {
        Role? role =
            await _roleRepository.GetRoleByRoleName(roleName, cancellationToken)
            ?? throw new NotFoundException("role not found");

        UserRole? userRole = await _dbContext.UserRoles.SingleOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == role.Id,
            cancellationToken
        );

        if (userRole is not null)
            return userRole;

        UserRole newUserRole = new() { RoleId = role.Id, UserId = userId };

        await _dbContext.UserRoles.AddAsync(newUserRole, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newUserRole;
    }
}
