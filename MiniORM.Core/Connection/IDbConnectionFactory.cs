using System.Data;
using System.Data.Common;

namespace MiniORM.Core.Connection;

/// <summary>
/// Factory interface for creating database connections.
/// 
/// Design Pattern: Abstract Factory
/// - Allows different implementations for different databases
/// </summary>
public interface IDbConnectionFactory
{
    #region Synchronous Methods

    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    IDbConnection CreateConnection();

    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    IDbConnection CreateOpenConnection();

    #endregion

    #region Asynchronous Methods

    /// <summary>
    /// Creates a new database connection asynchronously.
    /// </summary>
    /// <remarks>
    /// Returns DbConnection instead of IDbConnection because IDbConnection 
    /// doesn't define async methods. DbConnection provides OpenAsync().
    /// </remarks>
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and opens a new database connection asynchronously.
    /// </summary>
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the database provider name.
    /// </summary>
    string ProviderName { get; }

    #endregion
}
