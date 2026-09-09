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

            UserRole? userRole = await _userRoleRepository.AddUserRole(
                newUser.Id,
                Roles.USER,
                cancellationToken
            );

            IReadOnlyList<Permission?> permissions =
                await _roleRepository.GetRolePermissionsByRoleId(
                    userRole.RoleId,
                    cancellationToken
                );

            (accessToken, _) = await _cryptoService.GenerateAccessToken(
                new AccessTokenClaims
                {
                    UserId = newUser.Id,
                    UserProfileId = newUser.UserProfile!.Id,
                    ActiveRole = Roles.USER,
                    Permissions = [.. permissions.Where(p => p is not null).Select(p => p!.Name)],
                    Roles = [Roles.USER],
                }
            );

            Guid sessionId = Guid.NewGuid();

            (refreshToken, DateTime expiresAt) = await _cryptoService.GenerateRefreshToken(
                new RefreshTokenClaims { SessionId = sessionId, UserRoleId = userRole.Id }
            );

            await _userSessionRepository.AddUserSession(
                newUser.Id,
                userRole.Id,
                _cryptoService.HashRefreshToken(refreshToken),
                expiresAt,
                sessionId,
                cancellationToken
            );
        });

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

        IReadOnlyList<string> permissionNames = await _roleRepository.GetPermissionNamesByRoleId(
            userSession.UserRole!.Role!.Id,
            cancellationToken
        );

        (string newAccessToken, _) = await _cryptoService.GenerateAccessToken(
            new AccessTokenClaims
            {
                ActiveRole = userSession.UserRole!.Role!.Name,
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
}
