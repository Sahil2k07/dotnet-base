using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("USER_ROLE", Schema = "dbo")]
public class UserRole : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    public required long UserId { get; set; }

    public required long RoleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; } = null;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }
}
