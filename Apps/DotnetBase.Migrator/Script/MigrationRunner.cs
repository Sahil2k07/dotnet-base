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
            rootPath: "Migrations/Tables",
            fileType: "TABLE",
            throwOnChange: true
        );

        IReadOnlyList<MigrationFile> indexes = await _fileService.GetMigrationFiles(
            rootPath: "Migrations/Indexes",
            fileType: "INDEX",
            throwOnChange: true
        );

        IReadOnlyList<MigrationFile> views = await _fileService.GetMigrationFiles(
            rootPath: "Migrations/Views",
            fileType: "VIEW",
            throwOnChange: false
        );

        IReadOnlyList<MigrationFile> functions = await _fileService.GetMigrationFiles(
            rootPath: "Migrations/Functions",
            fileType: "FUNCTION",
            throwOnChange: false
        );

        IReadOnlyList<MigrationFile> procedures = await _fileService.GetMigrationFiles(
            rootPath: "Migrations/Procedures",
            fileType: "PROCEDURE",
            throwOnChange: false
        );

        IReadOnlyList<MigrationFile> triggers = await _fileService.GetMigrationFiles(
            rootPath: "Migrations/Triggers",
            fileType: "TRIGGER",
            throwOnChange: false
        );

        int totalPendingCount =
            tables.Count
            + indexes.Count
            + views.Count
            + functions.Count
            + procedures.Count
            + triggers.Count;

        _logger.LogInformation(
            "Migration SQL scripts read. Pending migrations: {Count}",
            totalPendingCount
        );

        await _sqlExecutor.ExecuteInTransactionAsync(
            async (_) =>
            {
                await _migrationExecutor.ApplyMigration(tables);
                await _migrationExecutor.ApplyMigration(indexes);
                await _migrationExecutor.ApplyMigration(views);
                await _migrationExecutor.ApplyMigration(functions);
                await _migrationExecutor.ApplyMigration(procedures);
                await _migrationExecutor.ApplyMigration(triggers);
            }
        );

        _logger.LogInformation(
            "Migration completed successfully in {Elapsed}. Total: {Total} "
                + "(Tables: {Tables}, Indexes: {Indexes}, Views: {Views}, "
                + "Functions: {Functions}, Procedures: {Procedures}, Triggers: {Triggers})",
            FormatElapsedTime(stopwatch.Elapsed),
            totalPendingCount,
            tables.Count,
            indexes.Count,
            views.Count,
            functions.Count,
            procedures.Count,
            triggers.Count
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