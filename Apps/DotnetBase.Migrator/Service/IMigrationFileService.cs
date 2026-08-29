using DotnetBase.Migrator.Helper;

namespace DotnetBase.Migrator.Service;

public interface IMigrationFileService
{
    Task<IReadOnlyList<MigrationFile>> GetMigrationFiles(
        string rootPath,
        string fileType,
        bool throwOnChange = false
    );
}