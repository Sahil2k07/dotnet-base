using System.Security.Cryptography;
using System.Text;
using DotnetBase.Migrator.Helper;
using Microsoft.Extensions.Logging;

namespace DotnetBase.Migrator.Service.Implementation;

public sealed class MigrationFileService : IMigrationFileService
{
    private readonly IMigrationHistoryService _historyService;

    private readonly ILogger<MigrationFileService> _logger;

    public MigrationFileService(
        IMigrationHistoryService historyService,
        ILogger<MigrationFileService> logger
    )
    {
        _historyService = historyService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MigrationFile>> GetMigrationFiles(
        string rootPath,
        string fileType,
        bool throwOnChange = true
    )
    {
        List<MigrationFile> migrationFiles = [];

        string migrationsFilesPath = Path.Combine(AppContext.BaseDirectory, rootPath);

        if (!Directory.Exists(migrationsFilesPath))
        {
            if (!Directory.Exists(migrationsFilesPath))
            {
                _logger.LogWarning("Migration directory not found: {Path}", rootPath);

                return migrationFiles;
            }
            return migrationFiles;
        }

        string[] files = Directory.GetFiles(
            migrationsFilesPath,
            "*.sql",
            SearchOption.AllDirectories
        );

        if (files.Length == 0)
        {
            return migrationFiles;
        }

        IReadOnlyList<MigrationHistory> migrationHistories =
            await _historyService.GetMigrationHistories();

        foreach (string file in files)
        {
            string sql = await File.ReadAllTextAsync(file);

            string fileHash = GenerateHash(sql);

            string filePath = Path.GetRelativePath(AppContext.BaseDirectory, file)
                .Replace('\\', '/');

            MigrationHistory? history = migrationHistories.FirstOrDefault(x =>
                string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase)
            );

            if (history is not null)
            {
                if (string.Equals(history.FileHash, fileHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Migration file was skipped. FileName: {FileName}. FilePath: {FilePath}",
                        Path.GetFileName(file),
                        filePath
                    );
                    continue;
                }

                if (throwOnChange)
                {
                    _logger.LogError(
                        "Migration file has changed after being applied. FilePath: {FilePath}",
                        filePath
                    );

                    throw new InvalidOperationException(
                        $"Migration file has changed after being applied: {filePath}"
                    );
                }
            }

            migrationFiles.Add(
                new MigrationFile
                {
                    FileName = Path.GetFileName(file),
                    FilePath = filePath,
                    FileHash = fileHash,
                    FileType = fileType,
                    SQL = sql,
                }
            );
        }

        return [.. migrationFiles.OrderBy(mf => mf.FilePath, StringComparer.OrdinalIgnoreCase)];
    }

    private static string GenerateHash(string sql)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(sql));

        return Convert.ToHexString(hash);
    }
}