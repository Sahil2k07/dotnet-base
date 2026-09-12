using DotnetBase.Data.Model;

namespace DotnetBase.Data.Repository;

public interface IRoleRepository
{
    Task<Role?> GetRoleByRoleName(string role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasRoleByUserIdAndRoleNameName(
        long userId,
        string roleName,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Permission?>> GetPermissionsByUserId(
        long userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasPermissionByUserIdAndPermissionName(
        long userId,
        string permissionName,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> GetPermissionNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    );
}
