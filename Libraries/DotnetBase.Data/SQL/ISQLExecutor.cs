using DotnetBase.Contract.Input;

namespace DotnetBase.Data.SQL;

public interface ISQLExecutor
{
    Task ExecuteAsync(
        string sql,
        IEnumerable<QueryParameter>? parameters = null,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<T> ExecuteAsync<T>(string sql, IEnumerable<QueryParameter>? parameters = null);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );
}