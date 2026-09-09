using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Data.Repository.Implementation;

public sealed class UserRepository : IUserRepository
{
    private readonly DotnetBaseContext _dbContext;

    public UserRepository(DotnetBaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsEmailRegistered(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Users.Where(u => u.Email == email).AnyAsync(cancellationToken);
    }

    public async Task<long> GetUserProfileIdByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserProfiles.Where(up => up.UserId == userId)
            .Select(up => up.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User> AddUser(
        string email,
        string passwordHash,
        CancellationToken cancellationToken = default
    )
    {
        User newUser = new()
        {
            Email = email,
            PasswordHash = passwordHash,
            UpdatedAt = DateTime.UtcNow,
        };

        await _dbContext.Users.AddAsync(newUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newUser;
    }

    public async Task<User> AddUser(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? displayName,
        string? phone,
        string? profilePicture,
        DateOnly? dateOfBirth,
        CancellationToken cancellationToken = default
    )
    {
        User newUser = new()
        {
            Email = email,
            PasswordHash = passwordHash,
            UpdatedAt = DateTime.UtcNow,
            UserProfile = new()
            {
                FirstName = firstName,
                LastName = lastName,
                DisplayName = displayName,
                Phone = phone,
                DateOfBirth = dateOfBirth,
                ProfilePicture = profilePicture,
                UpdatedAt = DateTime.UtcNow,
            },
        };

        await _dbContext.AddAsync(newUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newUser;
    }
}
