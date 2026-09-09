using DotnetBase.Data.Model;

namespace DotnetBase.Data.Repository;

public interface IUserRoleRepository
{
    Task<UserRole> AddUserRole(
        long userId,
        string roleName,
        CancellationToken cancellationToken = default
    );
}
