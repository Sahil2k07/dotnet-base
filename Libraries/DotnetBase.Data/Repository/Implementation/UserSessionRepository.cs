using System.Security.Authentication;
using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Data.Repository.Implementation;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly DotnetBaseContext _dbContext;

    public UserSessionRepository(DotnetBaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSession?> GetUserSessionByUserSessionId(
        Guid sessionId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.UserSessions.FirstOrDefaultAsync(
            us => us.DisplayId == sessionId,
            cancellationToken
        );
    }

    public async Task<UserSession> AddUserSession(
        long userId,
        string sessionTokenHash,
        DateTime expiresAt,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default
    )
    {
        UserSession userSession = new()
        {
            UserId = userId,
            SessionTokenHash = sessionTokenHash,
            DisplayId = sessionId ?? Guid.NewGuid(),
            ExpiresAt = expiresAt,
        };

        await _dbContext.UserSessions.AddAsync(userSession, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userSession;
    }

    public async Task<UserSession> UpdateUserSessionByUserSessionId(
        Guid sessionId,
        string sessionTokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    )
    {
        UserSession? userSession =
            await GetUserSessionByUserSessionId(sessionId, cancellationToken)
            ?? throw new AuthenticationException("invalid session id");

        userSession.SessionTokenHash = sessionTokenHash;
        userSession.ExpiresAt = expiresAt;

        _dbContext.UserSessions.Update(userSession);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userSession;
    }

    public async Task UpdateUserSession(
        UserSession userSession,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.UserSessions.Update(userSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserSessionByUserSessionId(
        Guid sessionId,
        CancellationToken cancellationToken = default
    )
    {
        UserSession? userSession = await GetUserSessionByUserSessionId(
            sessionId,
            cancellationToken
        );

        if (userSession is null)
            return;

        _dbContext.UserSessions.Remove(userSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserSession(
        UserSession userSession,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.UserSessions.Remove(userSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanUpExpiredSessionsByUserId(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext
            .UserSessions.Where(us =>
                us.UserId == userId && us.ExpiresAt < DateTime.UtcNow.AddDays(-30)
            )
            .ExecuteDeleteAsync(cancellationToken);
    }
}
