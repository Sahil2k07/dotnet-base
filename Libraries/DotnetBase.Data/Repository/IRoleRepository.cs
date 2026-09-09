using DotnetBase.Data.Model;

namespace DotnetBase.Data.Repository;

public interface IRoleRepository
{
    Task<Role?> GetRoleByRoleName(string role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission?>> GetRolePermissionsByRoleId(
        long roleId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> GetRoleNamesByUserId(
        long userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> GetPermissionNamesByRoleId(
        long roleId,
        CancellationToken cancellationToken = default
    );
}
