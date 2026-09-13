using DotnetBase.Authentication.Service;
using DotnetBase.Service.User;
using Moq;

namespace DotnetBase.Server.UnitTest.Service;

public sealed class UserServiceTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _userService = new UserService(_currentUserMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserInformation_WithValidUser_ReturnsUserInformation()
    {
        var roles = new List<string> { "USER", "MANAGER" };

        var permissions = new List<string> { "USER_READ", "USER_CREATE" };

        _currentUserMock.Setup(x => x.UserId).Returns(123);

        _currentUserMock.Setup(x => x.UserProfileId).Returns(456);

        _currentUserMock.Setup(x => x.GetRoles()).ReturnsAsync(roles);

        _currentUserMock.Setup(x => x.GetPermissions()).ReturnsAsync(permissions);

        var result = await _userService.GetCurrentUserInformation();

        Assert.NotNull(result);

        Assert.Equal(123, result.UserId);
        Assert.Equal(456, result.UserProfileId);

        Assert.Equal(roles, result.Roles);
        Assert.Equal(permissions, result.Permissions);

        _currentUserMock.Verify(x => x.GetRoles(), Times.Once);

        _currentUserMock.Verify(x => x.GetPermissions(), Times.Once);
    }
}
