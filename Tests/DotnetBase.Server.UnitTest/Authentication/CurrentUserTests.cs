using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetBase.Authentication.Service.Implementation;
using DotnetBase.Data.Repository;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DotnetBase.Server.UnitTest.Authentication;

public sealed class CurrentUserTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly Mock<IRoleRepository> _roleRepositoryMock;

    private readonly CurrentUser _currentUser;

    public CurrentUserTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _currentUser = new CurrentUser(_httpContextAccessorMock.Object, _roleRepositoryMock.Object);
    }

    private void SetupUser(long userId = 123, long userProfileId = 456, DateTime? expiresAt = null)
    {
        var expiration = expiresAt ?? DateTime.UtcNow.AddMinutes(15);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("user_profile_id", userProfileId.ToString()),
            new(
                JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString()
            ),
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        SetupUser();

        Assert.True(_currentUser.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WithoutHttpContext_ReturnsFalse()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        Assert.False(_currentUser.IsAuthenticated);
    }

    [Fact]
    public void UserId_WithValidClaim_ReturnsUserId()
    {
        SetupUser(userId: 123);

        Assert.Equal(123, _currentUser.UserId);
    }

    [Fact]
    public void UserProfileId_WithValidClaim_ReturnsUserProfileId()
    {
        SetupUser(userProfileId: 456);

        Assert.Equal(456, _currentUser.UserProfileId);
    }

    [Fact]
    public void ExpiresAt_WithValidClaim_ReturnsExpirationTime()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        SetupUser(expiresAt: expiresAt);

        Assert.Equal(
            new DateTimeOffset(expiresAt).ToUnixTimeSeconds(),
            new DateTimeOffset(_currentUser.ExpiresAt).ToUnixTimeSeconds()
        );
    }

    [Fact]
    public void ExpiresAt_WithoutExpirationClaim_ReturnsMinValue()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "123"),
            new("user_profile_id", "456"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        Assert.Equal(DateTime.MinValue, _currentUser.ExpiresAt);
    }

    [Fact]
    public void GetAccessTokenClaims_WithValidClaims_ReturnsClaims()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        SetupUser(userId: 123, userProfileId: 456, expiresAt: expiresAt);

        var result = _currentUser.GetAccessTokenClaims();

        Assert.Equal(123, result.UserId);
        Assert.Equal(456, result.UserProfileId);

        Assert.Equal(
            new DateTimeOffset(expiresAt).ToUnixTimeSeconds(),
            new DateTimeOffset(result.ExpiresAt).ToUnixTimeSeconds()
        );
    }

    [Fact]
    public async Task GetRoles_WithValidUser_ReturnsRoles()
    {
        SetupUser(userId: 123);

        var roles = new List<string> { "USER", "MANAGER" };

        _roleRepositoryMock.Setup(x => x.GetRoleNamesByUserId(123)).ReturnsAsync(roles);

        var result = await _currentUser.GetRoles();

        Assert.Equal(roles, result);

        _roleRepositoryMock.Verify(x => x.GetRoleNamesByUserId(123), Times.Once);
    }

    [Fact]
    public async Task HasRole_WithValidUser_ReturnsTrue()
    {
        SetupUser(userId: 123);

        _roleRepositoryMock
            .Setup(x => x.HasRoleByUserIdAndRoleNameName(123, "MANAGER"))
            .ReturnsAsync(true);

        var result = await _currentUser.HasRole("MANAGER");

        Assert.True(result);

        _roleRepositoryMock.Verify(
            x => x.HasRoleByUserIdAndRoleNameName(123, "MANAGER"),
            Times.Once
        );
    }

    [Fact]
    public async Task HasRole_WithUserWithoutRole_ReturnsFalse()
    {
        SetupUser(userId: 123);

        _roleRepositoryMock
            .Setup(x => x.HasRoleByUserIdAndRoleNameName(123, "ADMIN"))
            .ReturnsAsync(false);

        var result = await _currentUser.HasRole("ADMIN");

        Assert.False(result);
    }

    [Fact]
    public async Task GetPermissions_WithValidUser_ReturnsPermissions()
    {
        SetupUser(userId: 123);

        var permissions = new List<string> { "USER_READ", "USER_CREATE" };

        _roleRepositoryMock.Setup(x => x.GetPermissionNamesByUserId(123)).ReturnsAsync(permissions);

        var result = await _currentUser.GetPermissions();

        Assert.Equal(permissions, result);

        _roleRepositoryMock.Verify(x => x.GetPermissionNamesByUserId(123), Times.Once);
    }

    [Fact]
    public async Task HasPermission_WithValidUser_ReturnsTrue()
    {
        SetupUser(userId: 123);

        _roleRepositoryMock
            .Setup(x => x.HasPermissionByUserIdAndPermissionName(123, "USER_READ"))
            .ReturnsAsync(true);

        var result = await _currentUser.HasPermission("USER_READ");

        Assert.True(result);

        _roleRepositoryMock.Verify(
            x => x.HasPermissionByUserIdAndPermissionName(123, "USER_READ"),
            Times.Once
        );
    }

    [Fact]
    public async Task HasPermission_WithUserWithoutPermission_ReturnsFalse()
    {
        SetupUser(userId: 123);

        _roleRepositoryMock
            .Setup(x => x.HasPermissionByUserIdAndPermissionName(123, "USER_DELETE"))
            .ReturnsAsync(false);

        var result = await _currentUser.HasPermission("USER_DELETE");

        Assert.False(result);
    }
}
