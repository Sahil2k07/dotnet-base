using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Authentication.Authorization.Handler;
using DotnetBase.Authentication.Authorization.Requirement;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace DotnetBase.Server.UnitTest.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;

    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _handler = new PermissionAuthorizationHandler(_roleRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnauthenticatedUser_DoesNotSucceed()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        var context = CreateContext(principal, "USER_READ");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasPermissionByUserIdAndPermissionName(
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

        var context = CreateContext(principal, "USER_READ");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasPermissionByUserIdAndPermissionName(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserDoesNotHavePermission_DoesNotSucceed()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasPermissionByUserIdAndPermissionName(
                    123,
                    "USER_READ",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var context = CreateContext(principal, "USER_READ");

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasPermissionByUserIdAndPermissionName(
                    123,
                    "USER_READ",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasPermission_SucceedsRequirement()
    {
        var principal = CreateAuthenticatedUser("123");

        _roleRepositoryMock
            .Setup(x =>
                x.HasPermissionByUserIdAndPermissionName(
                    123,
                    "USER_READ",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var context = CreateContext(principal, "USER_READ");

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);

        _roleRepositoryMock.Verify(
            x =>
                x.HasPermissionByUserIdAndPermissionName(
                    123,
                    "USER_READ",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static AuthorizationHandlerContext CreateContext(
        ClaimsPrincipal user,
        string permission
    )
    {
        var requirement = new PermissionRequirement(permission);

        return new AuthorizationHandlerContext(new[] { requirement }, user, null);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string userId)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new ClaimsPrincipal(identity);
    }
}
