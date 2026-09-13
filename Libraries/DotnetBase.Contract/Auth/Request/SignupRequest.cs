using System.ComponentModel.DataAnnotations;

namespace DotnetBase.Contract.Auth.Request;

public sealed class SignupRequest
{
    [EmailAddress]
    [MaxLength(100)]
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [MaxLength(50)]
    [Required]
    public required string FirstName { get; set; }

    [MaxLength(50)]
    [Required]
    public required string LastName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    public DateOnly? DateOfBirth { get; set; } = null;
}
