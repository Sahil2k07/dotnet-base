using DotnetBase.Data.Model;

namespace DotnetBase.Data.Repository;

public interface IUserRepository
{
    Task<bool> IsEmailRegistered(string email, CancellationToken cancellationToken = default);

    Task<long> GetUserProfileIdByUserId(long userId, CancellationToken cancellationToken = default);

    Task<User> AddUser(
        string email,
        string passwordHash,
        CancellationToken cancellationToken = default
    );

    Task<User> AddUser(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? displayName,
        string? phone,
        string? profilePicture,
        DateOnly? dateOfBirth,
        CancellationToken cancellationToken = default
    );
}
