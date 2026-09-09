using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;

namespace DotnetBase.Service.Auth;

public interface IAuthService
{
    Task<SigninResponse> SignupUser(
        SignupRequest request,
        CancellationToken cancellationToken = default
    );

    Task<SigninResponse> SigninUser(
        SigninRequest request,
        CancellationToken cancellationToken = default
    );

    Task<SigninResponse> RefreshAccessToken(
        string refreshToken,
        CancellationToken cancellationToken = default
    );

    Task SignoutUser(string refreshToken, CancellationToken cancellationToken = default);
}
