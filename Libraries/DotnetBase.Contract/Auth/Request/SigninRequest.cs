using System.ComponentModel.DataAnnotations;

namespace DotnetBase.Contract.Auth.Request;

public sealed class SigninRequest
{
    [EmailAddress]
    public required string Email { get; set; }

    public required string Password { get; set; }
}
