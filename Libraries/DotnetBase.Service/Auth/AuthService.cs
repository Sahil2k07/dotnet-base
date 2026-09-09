using DotnetBase.Authentication.Service;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Data.Model;
using DotnetBase.Data.Repository;
using DotnetBase.Data.SQL;
using DotnetBase.Shared.Constant;
using DotnetBase.Shared.Exceptions;

namespace DotnetBase.Service.Auth;

public sealed class AuthService : IAuthService
{
    private readonly ISQLExecutor _sqlExecutor;

    private readonly IUserRepository _userRepository;

    private readonly ICryptoService _cryptoService;

    private readonly IRoleRepository _roleRepository;

    private readonly IUserRoleRepository _userRoleRepository;

    private readonly IUserSessionRepository _userSessionRepository;

    public AuthService(
        ISQLExecutor sqlExecutor,
        IUserRepository userRepository,
        ICryptoService cryptoService,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUserSessionRepository userSessionRepository
    )
    {
        _sqlExecutor = sqlExecutor;
        _userRepository = userRepository;
        _cryptoService = cryptoService;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _userSessionRepository = userSessionRepository;
    }

    public async Task<SigninResponse> SignupUser(
        SignupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        bool emailAlreadyRegistered = await _userRepository.IsEmailRegistered(
            request.Email,
            cancellationToken
        );

        if (emailAlreadyRegistered)
            throw new ConflictException("Email is Already registered");

        string passwordHash = _cryptoService.GeneratePasswordHash(request.Password);

        string accessToken = string.Empty;
        string refreshToken = string.Empty;

        await _sqlExecutor.ExecuteInTransactionAsync(async cancellationToken =>
        {
            User newUser = await _userRepository.AddUser(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.Phone,
                null,
                request.DateOfBirth,
                cancellationToken
            );

            await _userRoleRepository.AddUserRole(newUser.Id, Roles.USER, cancellationToken);

            IReadOnlyList<string> permissions = await _roleRepository.GetPermissionNamesByUserId(
                newUser.Id,
                cancellationToken
            );

            (accessToken, _) = await _cryptoService.GenerateAccessToken(
                new AccessTokenClaims
                {
                    UserId = newUser.Id,
                    UserProfileId = newUser.UserProfile!.Id,
                    Permissions = permissions,
                    Roles = [Roles.USER],
                }
            );

            Guid sessionId = Guid.NewGuid();

            (refreshToken, DateTime expiresAt) = await _cryptoService.GenerateRefreshToken(
                new RefreshTokenClaims { SessionId = sessionId }
            );

            await _userSessionRepository.AddUserSession(
                newUser.Id,
                _cryptoService.HashRefreshToken(refreshToken),
                expiresAt,
                sessionId,
                cancellationToken
            );
        });

        return new SigninResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<SigninResponse> SigninUser(
        SigninRequest request,
        CancellationToken cancellationToken = default
    )
    {
        User? user =
            await _userRepository.GetUserWithUserProfileByEmail(request.Email, cancellationToken)
            ?? throw new NotFoundException("email not recognized");

        bool isPasswordLegit = _cryptoService.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordLegit)
            throw new BadRequestException("wrong password");

        IReadOnlyList<string> roles = await _roleRepository.GetRoleNamesByUserId(
            user.Id,
            cancellationToken
        );

        IReadOnlyList<string> permissions = await _roleRepository.GetPermissionNamesByUserId(
            user.Id,
            cancellationToken
        );

        (string accessToken, _) = await _cryptoService.GenerateAccessToken(
            new AccessTokenClaims
            {
                UserId = user.Id,
                UserProfileId = user.UserProfile!.Id,
                Permissions = permissions,
                Roles = roles,
            }
        );

        Guid sessionId = Guid.NewGuid();

        (string refreshToken, DateTime expiresAt) = await _cryptoService.GenerateRefreshToken(
            new RefreshTokenClaims { SessionId = sessionId }
        );

        await _userSessionRepository.AddUserSession(
            user.Id,
            _cryptoService.HashRefreshToken(refreshToken),
            expiresAt,
            sessionId,
            cancellationToken
        );

        // Let's perform some cleanup here
        await _userSessionRepository.CleanUpExpiredSessionsByUserId(user.Id, cancellationToken);

        return new SigninResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<SigninResponse> RefreshAccessToken(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        RefreshTokenClaims refreshTokenClaims = await _cryptoService.GetRefreshTokenClaims(
            refreshToken
        );

        UserSession? userSession =
            await _userSessionRepository.GetUserSession(
                refreshTokenClaims.SessionId,
                cancellationToken
            ) ?? throw new AuthenticationException("malformed refresh token");

        if (
            refreshTokenClaims.ExpiresAt <= DateTime.UtcNow
            || userSession.ExpiresAt <= DateTime.UtcNow
            || userSession.RevokedAt <= DateTime.UtcNow
        )
        {
            await _userSessionRepository.DeleteUserSession(userSession, cancellationToken);

            throw new AuthenticationException("user session expired");
        }

        if (!_cryptoService.VerifyRefreshTokenHash(refreshToken, userSession.SessionTokenHash))
            throw new AuthenticationException("invalid refresh token");

        IReadOnlyList<string> roleNames = await _roleRepository.GetRoleNamesByUserId(
            userSession.UserId,
            cancellationToken
        );

        IReadOnlyList<string> permissionNames = await _roleRepository.GetPermissionNamesByUserId(
            userSession.UserId,
            cancellationToken
        );

        (string newAccessToken, _) = await _cryptoService.GenerateAccessToken(
            new AccessTokenClaims
            {
                Permissions = permissionNames,
                UserId = userSession.UserId,
                Roles = roleNames,
                UserProfileId = await _userRepository.GetUserProfileIdByUserId(userSession.UserId),
            }
        );

        (string newRefreshToken, DateTime expiresAt) = await _cryptoService.GenerateRefreshToken(
            new RefreshTokenClaims { SessionId = userSession.DisplayId }
        );

        userSession.ExpiresAt = expiresAt;
        userSession.SessionTokenHash = _cryptoService.HashRefreshToken(newRefreshToken);
        userSession.LastUsedAt = DateTime.UtcNow;

        await _userSessionRepository.UpdateUserSession(userSession, cancellationToken);

        return new SigninResponse { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
    }

    public async Task SignoutUser(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        RefreshTokenClaims refreshTokenClaims = await _cryptoService.GetRefreshTokenClaims(
            refreshToken
        );

        UserSession? userSession =
            await _userSessionRepository.GetUserSession(
                refreshTokenClaims.SessionId,
                cancellationToken
            ) ?? throw new AuthenticationException("malformed refresh token");

        await _userSessionRepository.DeleteUserSession(userSession, cancellationToken);

        // Let's perform some cleanup here
        await _userSessionRepository.CleanUpExpiredSessionsByUserId(
            userSession.UserId,
            cancellationToken
        );
    }
}
