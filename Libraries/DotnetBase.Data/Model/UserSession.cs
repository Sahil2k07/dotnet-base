using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("USER_SESSION", Schema = "dbo")]
public class UserSession : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long UserRoleId { get; set; }

    public Guid DisplayId { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public required string SessionTokenHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(UserRoleId))]
    public virtual UserRole? UserRole { get; set; }
}
