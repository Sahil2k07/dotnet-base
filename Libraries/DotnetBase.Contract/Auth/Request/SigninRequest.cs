using System.ComponentModel.DataAnnotations;

namespace DotnetBase.Contract.Auth.Request;

public sealed class SigninRequest
{
    [EmailAddress]
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
