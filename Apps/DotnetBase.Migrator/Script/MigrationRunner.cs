using System.Diagnostics;
using DotnetBase.Data.SQL;
using DotnetBase.Migrator.Helper;
using DotnetBase.Migrator.Service;
using Microsoft.Extensions.Logging;

namespace DotnetBase.Migrator.Script;

public sealed class MigrationRunner : IMigrationRunner
{
    private readonly ISQLExecutor _sqlExecutor;

    private readonly IMigrationFileService _fileService;

    private readonly IMigrationExecutor _migrationExecutor;

    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(
        ISQLExecutor sqlExecutor,
        IMigrationFileService fileService,
        IMigrationExecutor migrationExecutor,
        ILogger<MigrationRunner> logger
    )
    {
        _sqlExecutor = sqlExecutor;
        _fileService = fileService;
        _migrationExecutor = migrationExecutor;
        _logger = logger;
    }

    public async Task RunMigrationAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        IReadOnlyList<MigrationFile> tables = await _fileService.GetMigrationFiles(
            rootPath: "Migration/Table",
            fileType: "TABLE",
            throwOnChange: true
        );

        IReadOnlyList<MigrationFile> indexes = await _fileService.GetMigrationFiles(
            rootPath: "Migration/Index",
            fileType: "INDEX",
            throwOnChange: true
        );

        IReadOnlyList<MigrationFile> scripts = await _fileService.GetMigrationFiles(
            rootPath: "Migration/Script",
            fileType: "SCRIPT",
            throwOnChange: false
        );

        int totalPendingCount = tables.Count + indexes.Count + scripts.Count;

        _logger.LogInformation(
            "Migration SQL scripts read. Pending migrations: {Count}",
            totalPendingCount
        );

        await _sqlExecutor.ExecuteInTransactionAsync(
            async (_) =>
            {
                await _migrationExecutor.ApplyMigration(tables);
                await _migrationExecutor.ApplyMigration(indexes);
                await _migrationExecutor.ApplyMigration(scripts);
            }
        );

        _logger.LogInformation(
            "Migration completed successfully in {Elapsed}. Total: {Total} "
                + "(Tables: {Tables}, Indexes: {Indexes}, Scripts: {Scripts}, ",
            FormatElapsedTime(stopwatch.Elapsed),
            totalPendingCount,
            tables.Count,
            indexes.Count,
            scripts.Count
        );
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
            return $"{elapsed.TotalHours:0.##} h";

        if (elapsed.TotalMinutes >= 1)
            return $"{elapsed.TotalMinutes:0.##} m";

        if (elapsed.TotalSeconds >= 1)
            return $"{elapsed.TotalSeconds:0.##} s";

        return $"{elapsed.TotalMilliseconds:0.##} ms";
    }
}
