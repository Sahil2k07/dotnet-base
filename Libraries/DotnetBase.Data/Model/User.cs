using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("USER", Schema = "dbo")]
public class User : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    [MaxLength(100)]
    public required string Email { get; set; }

    [MaxLength(150)]
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; } = null;

    public virtual UserProfile? UserProfile { get; set; }

    public virtual ICollection<UserSession>? UserSessions { get; set; }

    public virtual ICollection<UserRole>? UserRoles { get; set; }
}
