namespace DotnetBase.Migrator.Helper;

public sealed class MigrationHistory
{
    public long ID { get; set; }

    public required string FileName { get; set; }

    public required string FilePath { get; set; }

    public required string FileType { get; set; }

    public required string FileHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}