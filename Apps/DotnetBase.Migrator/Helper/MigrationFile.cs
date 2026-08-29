namespace DotnetBase.Migrator.Helper;

public sealed class MigrationFile
{
    public required string FileName { get; set; }

    public required string FilePath { get; set; }

    public required string FileHash { get; set; }

    public required string FileType { get; set; }

    public required string SQL { get; set; }
}