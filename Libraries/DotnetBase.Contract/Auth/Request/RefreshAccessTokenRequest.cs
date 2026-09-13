using System.ComponentModel.DataAnnotations;

namespace DotnetBase.Contract.Auth.Request;

public sealed class RefreshAccessTokenRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
