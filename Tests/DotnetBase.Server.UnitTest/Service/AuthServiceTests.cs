using DotnetBase.Authentication.Service;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Data.Model;
using DotnetBase.Data.Repository;
using DotnetBase.Data.SQL;
using DotnetBase.Service.Auth;
using DotnetBase.Shared.Constant;
using DotnetBase.Shared.Exceptions;
using Moq;

namespace DotnetBase.Server.UnitTest.Service;

public sealed class AuthServiceTests
{
    private readonly Mock<ISQLExecutor> _sqlExecutorMock;

    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly Mock<ICryptoService> _cryptoServiceMock;

    private readonly Mock<IRoleRepository> _roleRepositoryMock;

    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;

    private readonly Mock<IUserSessionRepository> _userSessionRepositoryMock;

    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _sqlExecutorMock = new Mock<ISQLExecutor>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _cryptoServiceMock = new Mock<ICryptoService>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _userSessionRepositoryMock = new Mock<IUserSessionRepository>();

        _authService = new AuthService(
            _sqlExecutorMock.Object,
            _userRepositoryMock.Object,
            _cryptoServiceMock.Object,
            _roleRepositoryMock.Object,
            _userRoleRepositoryMock.Object,
            _userSessionRepositoryMock.Object
        );
    }

    [Fact]
    public async Task SignupUser_WithExistingEmail_ThrowsConflictException()
    {
        var request = new SignupRequest
        {
            Email = "test@example.com",
            Password = "Password@123",
            FirstName = "Test",
            LastName = "User",
        };

        _userRepositoryMock
            .Setup(x => x.IsEmailRegistered(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _authService.SignupUser(request)
        );

        Assert.Equal("Email is Already registered", exception.Message);

        _cryptoServiceMock.Verify(x => x.GeneratePasswordHash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignupUser_WithValidRequest_ReturnsTokens()
    {
        var request = new SignupRequest
        {
            Email = "test@example.com",
            Password = "Password@123",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            Phone = "9999999999",
        };

        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            PasswordHash = "password-hash",
            UpdatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile
            {
                Id = 456,
                FirstName = "Test",
                LastName = "User",
                UpdatedAt = DateTime.UtcNow,
            },
        };

        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        _userRepositoryMock
            .Setup(x => x.IsEmailRegistered(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _cryptoServiceMock
            .Setup(x => x.GeneratePasswordHash(request.Password))
            .Returns("password-hash");

        _sqlExecutorMock
            .Setup(x =>
                x.ExecuteInTransactionAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (Func<CancellationToken, Task> action, CancellationToken cancellationToken) =>
                    action(cancellationToken)
            );

        _userRepositoryMock
            .Setup(x =>
                x.AddUser(
                    request.Email,
                    "password-hash",
                    request.FirstName,
                    request.LastName,
                    request.DisplayName,
                    request.Phone,
                    null,
                    request.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _cryptoServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<AccessTokenClaims>()))
            .ReturnsAsync(("access-token", DateTime.UtcNow.AddMinutes(15)));

        _cryptoServiceMock
            .Setup(x => x.GenerateRefreshToken(It.IsAny<RefreshTokenClaims>()))
            .ReturnsAsync(("refresh-token", expiresAt));

        _cryptoServiceMock
            .Setup(x => x.HashRefreshToken("refresh-token"))
            .Returns("refresh-token-hash");

        var result = await _authService.SignupUser(request);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        _userRoleRepositoryMock.Verify(
            x => x.AddUserRole(123, Roles.USER, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userSessionRepositoryMock.Verify(
            x =>
                x.AddUserSession(
                    123,
                    "refresh-token-hash",
                    expiresAt,
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SigninUser_WithUnknownEmail_ThrowsNotFoundException()
    {
        var request = new SigninRequest
        {
            Email = "unknown@example.com",
            Password = "Password@123",
        };

        _userRepositoryMock
            .Setup(x =>
                x.GetUserWithUserProfileByEmail(request.Email, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _authService.SigninUser(request)
        );

        Assert.Equal("email not recognized", exception.Message);
    }

    [Fact]
    public async Task SigninUser_WithWrongPassword_ThrowsBadRequestException()
    {
        var request = new SigninRequest { Email = "test@example.com", Password = "WrongPassword" };

        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            PasswordHash = "password-hash",
            UpdatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile
            {
                Id = 456,
                FirstName = "Test",
                LastName = "User",
                UpdatedAt = DateTime.UtcNow,
            },
        };

        _userRepositoryMock
            .Setup(x =>
                x.GetUserWithUserProfileByEmail(request.Email, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);

        _cryptoServiceMock
            .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.SigninUser(request)
        );

        Assert.Equal("wrong password", exception.Message);
    }

    [Fact]
    public async Task SigninUser_WithValidCredentials_ReturnsTokens()
    {
        var request = new SigninRequest { Email = "test@example.com", Password = "Password@123" };

        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            PasswordHash = "password-hash",
            UpdatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile
            {
                Id = 456,
                FirstName = "Test",
                LastName = "User",
                UpdatedAt = DateTime.UtcNow,
            },
        };
        var expiresAt = DateTime.UtcNow.AddDays(7);

        _userRepositoryMock
            .Setup(x =>
                x.GetUserWithUserProfileByEmail(request.Email, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);

        _cryptoServiceMock
            .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        _cryptoServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<AccessTokenClaims>()))
            .ReturnsAsync(("access-token", DateTime.UtcNow.AddMinutes(15)));

        _cryptoServiceMock
            .Setup(x => x.GenerateRefreshToken(It.IsAny<RefreshTokenClaims>()))
            .ReturnsAsync(("refresh-token", expiresAt));

        _cryptoServiceMock
            .Setup(x => x.HashRefreshToken("refresh-token"))
            .Returns("refresh-token-hash");

        var result = await _authService.SigninUser(request);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        _userSessionRepositoryMock.Verify(
            x =>
                x.AddUserSession(
                    123,
                    "refresh-token-hash",
                    expiresAt,
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _userSessionRepositoryMock.Verify(
            x => x.CleanUpExpiredSessionsByUserId(123, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccessToken_WithInvalidRefreshToken_ThrowsAuthenticationException()
    {
        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("invalid-token"))
            .ThrowsAsync(new AuthenticationException("malformed token"));

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.RefreshAccessToken("invalid-token")
        );

        Assert.Equal("malformed token", exception.Message);
    }

    [Fact]
    public async Task RefreshAccessToken_WithMissingSession_ThrowsAuthenticationException()
    {
        var sessionId = Guid.NewGuid();

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(
                new RefreshTokenClaims
                {
                    SessionId = sessionId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                }
            );

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.RefreshAccessToken("refresh-token")
        );

        Assert.Equal("malformed refresh token", exception.Message);
    }

    [Fact]
    public async Task RefreshAccessToken_WithExpiredSession_DeletesSessionAndThrows()
    {
        var sessionId = Guid.NewGuid();

        var session = new UserSession
        {
            DisplayId = sessionId,
            UserId = 123,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            RevokedAt = null,
            SessionTokenHash = "hash",
        };

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(
                new RefreshTokenClaims
                {
                    SessionId = sessionId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                }
            );

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.RefreshAccessToken("refresh-token")
        );

        Assert.Equal("user session expired", exception.Message);

        _userSessionRepositoryMock.Verify(
            x => x.DeleteUserSession(session, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccessToken_WithInvalidHash_ThrowsAuthenticationException()
    {
        var sessionId = Guid.NewGuid();

        var session = new UserSession
        {
            DisplayId = sessionId,
            UserId = 123,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null,
            SessionTokenHash = "stored-hash",
        };

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(
                new RefreshTokenClaims
                {
                    SessionId = sessionId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                }
            );

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _cryptoServiceMock
            .Setup(x => x.VerifyRefreshTokenHash("refresh-token", "stored-hash"))
            .Returns(false);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.RefreshAccessToken("refresh-token")
        );

        Assert.Equal("invalid refresh token", exception.Message);
    }

    [Fact]
    public async Task RefreshAccessToken_WithValidToken_ReturnsNewTokens()
    {
        var sessionId = Guid.NewGuid();

        var session = new UserSession
        {
            DisplayId = sessionId,
            UserId = 123,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null,
            SessionTokenHash = "old-hash",
        };

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(
                new RefreshTokenClaims
                {
                    SessionId = sessionId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                }
            );

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _cryptoServiceMock
            .Setup(x => x.VerifyRefreshTokenHash("refresh-token", "old-hash"))
            .Returns(true);

        _userRepositoryMock.Setup(x => x.GetUserProfileIdByUserId(123)).ReturnsAsync(456);

        _cryptoServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<AccessTokenClaims>()))
            .ReturnsAsync(("new-access-token", DateTime.UtcNow.AddMinutes(15)));

        var newExpiresAt = DateTime.UtcNow.AddDays(7);

        _cryptoServiceMock
            .Setup(x => x.GenerateRefreshToken(It.IsAny<RefreshTokenClaims>()))
            .ReturnsAsync(("new-refresh-token", newExpiresAt));

        _cryptoServiceMock.Setup(x => x.HashRefreshToken("new-refresh-token")).Returns("new-hash");

        var result = await _authService.RefreshAccessToken("refresh-token");

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);

        Assert.Equal("new-hash", session.SessionTokenHash);
        Assert.Equal(newExpiresAt, session.ExpiresAt);

        _userSessionRepositoryMock.Verify(
            x => x.UpdateUserSession(session, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SignoutUser_WithMissingSession_ThrowsAuthenticationException()
    {
        var sessionId = Guid.NewGuid();

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(new RefreshTokenClaims { SessionId = sessionId });

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.SignoutUser("refresh-token")
        );

        Assert.Equal("malformed refresh token", exception.Message);
    }

    [Fact]
    public async Task SignoutUser_WithValidSession_DeletesSessionAndCleansUp()
    {
        var sessionId = Guid.NewGuid();

        var session = new UserSession
        {
            DisplayId = sessionId,
            UserId = 123,
            SessionTokenHash = "xxx-xxx-xxxxx",
        };

        _cryptoServiceMock
            .Setup(x => x.GetRefreshTokenClaims("refresh-token"))
            .ReturnsAsync(new RefreshTokenClaims { SessionId = sessionId });

        _userSessionRepositoryMock
            .Setup(x => x.GetUserSessionByUserSessionId(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _authService.SignoutUser("refresh-token");

        _userSessionRepositoryMock.Verify(
            x => x.DeleteUserSession(session, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userSessionRepositoryMock.Verify(
            x => x.CleanUpExpiredSessionsByUserId(123, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
