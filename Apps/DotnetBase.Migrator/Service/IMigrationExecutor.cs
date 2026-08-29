using DotnetBase.Migrator.Helper;

namespace DotnetBase.Migrator.Service;

public interface IMigrationExecutor
{
    Task ApplyMigration(IReadOnlyList<MigrationFile> migrationFiles);
}