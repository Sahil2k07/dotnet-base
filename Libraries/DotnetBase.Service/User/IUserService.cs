using DotnetBase.Contract.User.Response;

namespace DotnetBase.Service.User;

public interface IUserService
{
    Task<UserInformationResponse> GetCurrentUserInformation();
}
