using DotnetBase.Contract.User.Response;
using DotnetBase.Service.User;
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
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [ApiResponse("User data fetched successfully")]
    public async Task<IActionResult> Me()
    {
        UserInformationResponse userInfo = await _userService.GetCurrentUserInformation();

        return Ok(userInfo);
    }
}
