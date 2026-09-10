using DotnetBase.Data.Model;

namespace DotnetBase.Data.Repository;

public interface IUserSessionRepository
{
    Task<UserSession?> GetUserSessionByUserSessionId(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );

    Task<UserSession> AddUserSession(
        long userId,
        string sessionTokenHash,
        DateTime expiresAt,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default
    );

    Task<UserSession> UpdateUserSessionByUserSessionId(
        Guid sessionId,
        string sessionTokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    );

    Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default);

    Task DeleteUserSessionByUserSessionId(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );

    Task DeleteUserSession(UserSession userSession, CancellationToken cancellationToken = default);

    Task CleanUpExpiredSessionsByUserId(long userId, CancellationToken cancellationToken = default);
}
