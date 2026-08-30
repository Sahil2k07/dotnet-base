using System.Linq.Expressions;
using DotnetBase.Data.Configuration;
using DotnetBase.Data.Model;
using DotnetBase.Data.Model.Interface;
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

        #region Soft Delete

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            var entity = modelBuilder.Entity(entityType.ClrType);

            var parameter = Expression.Parameter(entityType.ClrType, "x");

            var property = Expression.Property(parameter, nameof(ISoftDelete.DeletedAt));

            var filter = Expression.Lambda(
                Expression.Equal(property, Expression.Constant(null, typeof(DateTime?))),
                parameter
            );

            entity.HasQueryFilter(filter);
        }

        #endregion

        modelBuilder.Entity<UserSession>().HasQueryFilter(x => x.User.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
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
