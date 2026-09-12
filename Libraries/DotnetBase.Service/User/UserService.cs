using DotnetBase.Authentication.Service;
using DotnetBase.Contract.User.Response;

namespace DotnetBase.Service.User;

public sealed class UserService : IUserService
{
    private readonly ICurrentUser _currentUser;

    public UserService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<UserInformationResponse> GetCurrentUserInformation()
    {
        IEnumerable<string> roles = await _currentUser.GetRoles();

        IEnumerable<string> permissions = await _currentUser.GetPermissions();

        return new UserInformationResponse
        {
            UserId = _currentUser.UserId,
            UserProfileId = _currentUser.UserProfileId,
            Roles = roles,
            Permissions = permissions,
        };
    }
}
