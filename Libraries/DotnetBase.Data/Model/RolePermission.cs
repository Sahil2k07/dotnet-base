using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("ROLE_PERMISSION", Schema = "dbo")]
public class RolePermission : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long PermissionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; } = null;

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public virtual Permission? Permission { get; set; }
}
