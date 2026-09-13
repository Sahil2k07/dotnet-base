using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Authentication.Authorization.Handler;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Moq;

namespace DotnetBase.Server.UnitTest.Authorization;

public sealed class RoleAuthorizationHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly RoleAuthorizationHandler _handler;

    public RoleAuthorizationHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _handler = new RoleAuthorizationHandler(_roleRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnauthenticatedUser_DoesNotSucceed()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        var context = CreateContext(principal, "ADMIN");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasRoleByUserIdAndRoleNameName(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidUserId_DoesNotSucceed()
    {
        var principal = CreateAuthenticatedUser("invalid-user-id");

        var context = CreateContext(principal, "ADMIN");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasRoleByUserIdAndRoleNameName(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserDoesNotHaveAllowedRole_DoesNotSucceed()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "MANAGER", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        var context = CreateContext(principal, "ADMIN", "MANAGER");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>()),
            Times.Once
        );

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "MANAGER", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasAllowedRole_SucceedsRequirement()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        var context = CreateContext(principal, "ADMIN");

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenSecondAllowedRoleMatches_SucceedsRequirement()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "MANAGER", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        var context = CreateContext(principal, "ADMIN", "MANAGER");

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>()),
            Times.Once
        );

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "MANAGER", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenFirstAllowedRoleMatches_DoesNotCheckRemainingRoles()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        var context = CreateContext(principal, "ADMIN", "MANAGER");

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "ADMIN", It.IsAny<CancellationToken>()),
            Times.Once
        );

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "MANAGER", It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static AuthorizationHandlerContext CreateContext(
        ClaimsPrincipal user,
        params string[] allowedRoles
    )
    {
        var requirement = new RolesAuthorizationRequirement(allowedRoles);

        return new AuthorizationHandlerContext(new[] { requirement }, user, null);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string userId)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new ClaimsPrincipal(identity);
    }
}
