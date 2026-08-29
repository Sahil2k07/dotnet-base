using DotnetBase.Contract.Input;
using DotnetBase.Data.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotnetBase.Data.SQL;

public sealed class SQLExecutor : ISQLExecutor
{
    private readonly DotnetBaseContext _dbContext;

    public SQLExecutor(DotnetBaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(
        string sql,
        IEnumerable<QueryParameter>? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        object[] sqlParameters = CreateSQLParameters(parameters);

        await _dbContext.Database.ExecuteSqlRawAsync(sql, sqlParameters, cancellationToken);
    }

    public IAsyncEnumerable<T> ExecuteAsync<T>(
        string sql,
        IEnumerable<QueryParameter>? parameters = null
    )
    {
        object[] sqlParameters = CreateSQLParameters(parameters);

        return _dbContext.Database.SqlQueryRaw<T>(sql, sqlParameters).AsAsyncEnumerable();
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    private static object[] CreateSQLParameters(IEnumerable<QueryParameter>? parameters = null)
    {
        if (parameters is null)
        {
            return [];
        }

        return
        [
            .. parameters
                .Select(p => new SqlParameter(p.ParamName, p.ParamValue ?? DBNull.Value))
                .Cast<object>(),
        ];
    }
}