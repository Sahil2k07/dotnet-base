using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetBase.Data.Model.Interface;

namespace DotnetBase.Data.Model;

[Table("ROLE", Schema = "dbo")]
public class Role : ISoftDelete
{
    [Key]
    public long Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; } = null;
}
