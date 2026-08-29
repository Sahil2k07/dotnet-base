using DotnetBase.Data.SQL;
using DotnetBase.Migrator.Helper;
using Microsoft.Extensions.Logging;

namespace DotnetBase.Migrator.Service.Implementation;

public sealed class MigrationExecutor : IMigrationExecutor
{
    private readonly ISQLExecutor _sqlExecutor;

    private readonly IMigrationHistoryService _historyService;

    private readonly ILogger<MigrationExecutor> _logger;

    public MigrationExecutor(
        ISQLExecutor sqlExecutor,
        IMigrationHistoryService historyService,
        ILogger<MigrationExecutor> logger
    )
    {
        _sqlExecutor = sqlExecutor;
        _historyService = historyService;
        _logger = logger;
    }

    public async Task ApplyMigration(IReadOnlyList<MigrationFile> migrationFiles)
    {
        List<MigrationHistory> migrationHistories = [];

        foreach (MigrationFile file in migrationFiles)
        {
            try
            {
                await _sqlExecutor.ExecuteAsync(file.SQL);

                _logger.LogInformation(
                    "Migration Applied. FileName: {FileName} FilePath: {FilePath}",
                    file.FileName,
                    file.FilePath
                );

                migrationHistories.Add(
                    new MigrationHistory
                    {
                        FileName = file.FileName,
                        FilePath = file.FilePath,
                        FileHash = file.FileHash,
                        FileType = file.FileType,
                    }
                );
            }
            catch (Exception)
            {
                _logger.LogError(
                    "Migration Failed. FileName: {FileName} FilePath: {FilePath}",
                    file.FileName,
                    file.FilePath
                );

                throw;
            }
        }

        await _historyService.SaveMigrationHistory(migrationHistories);
    }
}