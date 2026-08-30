using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("USER_PROFILE", Schema = "dbo")]
public class UserProfile : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }

    [MaxLength(50)]
    public required string FirstName { get; set; }

    [MaxLength(50)]
    public required string LastName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? ProfilePicture { get; set; }

    public DateOnly? DateOfBirth { get; set; } = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; } = null;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
