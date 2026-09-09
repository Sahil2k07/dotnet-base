using DotnetBase.Authentication.Service;
using DotnetBase.Contract.Auth.Claims;
using DotnetBase.Shared.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBase.Server.Controller;

[ApiController]
[Authorize]
[ApiResponse]
[Route("api/v1/[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public UserController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    [ApiResponse("User data fetched successfully")]
    public async Task<IActionResult> Me()
    {
        AccessTokenClaims accessTokenClaims = _currentUser.GetAccessTokenClaims();

        return Ok(accessTokenClaims);
    }
}
