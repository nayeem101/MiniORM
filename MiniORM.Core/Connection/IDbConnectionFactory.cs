using System.Data;

namespace MiniORM.Core.Connection;

/// <summary>
/// Factory interface for creating database connections.
/// 
/// Design Pattern: Abstract Factory
/// - Allows different implementations for different databases
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    IDbConnection CreateConnection();

    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    IDbConnection CreateOpenConnection();

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the database provider name.
    /// </summary>
    string ProviderName { get; }
}
