using DotnetBase.Data.Configuration;
using DotnetBase.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetBase.Data.Context;

public sealed class DotnetBaseContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<UserSession> UserSessions { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    private readonly DatabaseOption _dbOptions;

    public DotnetBaseContext(
        DbContextOptions<DotnetBaseContext> options,
        IOptions<DatabaseOption> dbOptions
    )
        : base(options)
    {
        _dbOptions = dbOptions.Value;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<UserProfile>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<UserRole>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<Role>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<Permission>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<RolePermission>().HasQueryFilter(x => x.DeletedAt == null);
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
