using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace MiniORM.Core.Connection;

/// <summary>
/// SQLite connection factory implementation.
/// 
/// Design Pattern: Factory Method
/// - Encapsulates connection creation logic
/// </summary>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    public string ConnectionString { get; }
    public string ProviderName => "Microsoft.Data.Sqlite";

    public SqliteConnectionFactory(string connectionString)
    {
        ConnectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates an in-memory SQLite database.
    /// </summary>
    public static SqliteConnectionFactory CreateInMemory()
    {
        return new SqliteConnectionFactory("Data Source=:memory:");
    }

    #region Synchronous Methods

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public IDbConnection CreateOpenConnection()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    #endregion

    #region Asynchronous Methods

    public Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DbConnection>(new SqliteConnection(ConnectionString));
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    #endregion
}
