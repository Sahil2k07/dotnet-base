namespace DotnetBase.Migrator.Script;

public interface IMigrationRunner
{
    Task RunMigrationAsync();
}