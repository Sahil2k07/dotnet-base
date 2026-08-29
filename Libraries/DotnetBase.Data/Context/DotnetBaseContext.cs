using DotnetBase.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetBase.Data.Context;

public sealed class DotnetBaseContext : DbContext
{
    private readonly DatabaseOption _dbOptions;

    public DotnetBaseContext(
        DbContextOptions<DotnetBaseContext> options,
        IOptions<DatabaseOption> dbOptions
    )
        : base(options)
    {
        _dbOptions = dbOptions.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            var connectionString =
                $"Server={_dbOptions.Server};"
                + $"Database={_dbOptions.Database};"
                + $"User Id={_dbOptions.Username};"
                + $"Password={_dbOptions.Password};"
                + $"TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}