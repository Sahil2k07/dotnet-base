using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Service.Auth;
using DotnetBase.Shared.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBase.Server.Controller;

[ApiController]
[Authorize]
[ApiResponse]
[Route("/api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    [ApiResponse("User signed up successful")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken
    )
    {
        SigninResponse response = await _authService.SignupUser(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("signin")]
    [ApiResponse("User signed in successfully")]
    [AllowAnonymous]
    public async Task<IActionResult> Signin(
        [FromBody] SigninRequest request,
        CancellationToken cancellationToken
    )
    {
        SigninResponse response = await _authService.SigninUser(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("signout")]
    [ApiResponse("User signed out successfully")]
    public async Task<IActionResult> Signout(
        [FromBody] RefreshAccessTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        await _authService.SignoutUser(request.RefreshToken, cancellationToken);

        return Ok();
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshAccessTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        SigninResponse response = await _authService.RefreshAccessToken(
            request.RefreshToken,
            cancellationToken
        );

        return Ok(response);
    }
}
