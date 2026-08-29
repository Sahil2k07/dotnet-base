using System.Text;
using DotnetBase.Contract.Input;
using DotnetBase.Data.SQL;
using DotnetBase.Migrator.Helper;
using Microsoft.Extensions.Logging;

namespace DotnetBase.Migrator.Service.Implementation;

public sealed class MigrationHistoryService : IMigrationHistoryService
{
    private readonly ISQLExecutor _sqlExecutor;

    private readonly ILogger<MigrationHistoryService> _logger;

    private IReadOnlyList<MigrationHistory>? _histories;

    public MigrationHistoryService(
        ISQLExecutor sqlExecutor,
        ILogger<MigrationHistoryService> logger
    )
    {
        _sqlExecutor = sqlExecutor;
        _logger = logger;
        _histories = null;
    }

    public async Task<IReadOnlyList<MigrationHistory>> GetMigrationHistories()
    {
        _histories ??= await LoadMigrationHistories();
        return _histories;
    }

    private async Task<IReadOnlyList<MigrationHistory>> LoadMigrationHistories()
    {
        try
        {
            await EnsureMigrationHistoryExists();

            _logger.LogInformation("Migration table exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure the migration history table exists.");
            throw;
        }

        try
        {
            return await _sqlExecutor
                .ExecuteAsync<MigrationHistory>(
                    """"
                    SELECT * FROM [migration].[MigrationHistory]
                    """"
                )
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load migration history");
            throw;
        }
        finally
        {
            _logger.LogInformation("Migration history loaded");
        }
    }

    public async Task SaveMigrationHistory(ICollection<MigrationHistory> migrationHistories)
    {
        if (migrationHistories.Count == 0)
            return;

        var values = new StringBuilder();
        var parameters = new List<QueryParameter>();

        var index = 0;

        foreach (var migration in migrationHistories)
        {
            if (index > 0)
                values.AppendLine(",");

            values.Append(
                $"(@FileName{index}, @FilePath{index}, " + $"@FileType{index}, @FileHash{index})"
            );

            parameters.Add(new QueryParameter($"FileName{index}", migration.FileName));

            parameters.Add(new QueryParameter($"FilePath{index}", migration.FilePath));

            parameters.Add(new QueryParameter($"FileType{index}", migration.FileType));

            parameters.Add(new QueryParameter($"FileHash{index}", migration.FileHash));

            index++;
        }

        var query = $"""
            UPDATE target
            SET
                target.FileHash = source.FileHash,
                target.AppliedAt = SYSUTCDATETIME()
            FROM [migration].[MigrationHistory] target
            INNER JOIN
            (
                VALUES
                {values}
            ) source (FileName, FilePath, FileType, FileHash)
                ON target.FilePath = source.FilePath;

            INSERT INTO [migration].[MigrationHistory]
            (
                FileName,
                FilePath,
                FileType,
                FileHash
            )
            SELECT
                source.FileName,
                source.FilePath,
                source.FileType,
                source.FileHash
            FROM
            (
                VALUES
                {values}
            ) source (FileName, FilePath, FileType, FileHash)
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM [migration].[MigrationHistory] target
                WHERE target.FilePath = source.FilePath
            );
            """;

        await _sqlExecutor.ExecuteAsync(query, parameters);
    }

    private async Task EnsureMigrationHistoryExists()
    {
        await _sqlExecutor.ExecuteAsync(
            """"
            IF NOT EXISTS (
                SELECT 1
                FROM sys.schemas
                WHERE name = 'migration'
            )
            BEGIN
                EXEC('CREATE SCHEMA [migration]')
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.tables t
                INNER JOIN sys.schemas s
                ON s.schema_id = t.schema_id
                WHERE s.name = 'migration' AND t.name = 'MigrationHistory'
            )
            BEGIN
                CREATE TABLE [migration].[MigrationHistory]
                (
                    ID        BIGINT IDENTITY(1, 1) NOT NULL,
                    FileName  NVARCHAR(150) NOT NULL,
                    FilePath  NVARCHAR(500) NOT NULL,
                    FileType  NVARCHAR(50) NOT NULL,
                    FileHash  NVARCHAR(100) NOT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
                    AppliedAt DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),

                    CONSTRAINT PK_MigrationHistory PRIMARY KEY (ID),
                    CONSTRAINT UQ_MigrationHistory_FilePath UNIQUE (FilePath)
                );
            END;
            """"
        );
    }
}