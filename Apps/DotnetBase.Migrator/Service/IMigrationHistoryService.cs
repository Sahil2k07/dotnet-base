using DotnetBase.Migrator.Helper;

namespace DotnetBase.Migrator.Service;

public interface IMigrationHistoryService
{
    Task<IReadOnlyList<MigrationHistory>> GetMigrationHistories();

    Task SaveMigrationHistory(ICollection<MigrationHistory> migrationHistories);
}