using System.Data;
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
}
