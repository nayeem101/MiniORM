using System.Data;
using MiniORM.Core.Connection;

namespace MiniORM.Core;

/// <summary>
/// Database context that manages connections and provides a gateway to the database.
/// 
/// Design Pattern: Singleton (optional) + Facade
/// - Provides a simplified interface to the ORM components
/// - Can be used as a singleton or instantiated per request
/// </summary>
public class DbContext : IDisposable
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    // Static instance for Singleton pattern (optional usage)
    private static DbContext? _instance;
    private static readonly object _lock = new object();

    public IDbConnectionFactory ConnectionFactory => _connectionFactory;

    /// <summary>
    /// Creates a new DbContext with the specified connection factory.
    /// </summary>
    public DbContext(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory
            ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Gets or creates a singleton instance.
    /// Thread-safe implementation using double-check locking.
    /// </summary>
    public static DbContext GetInstance(IDbConnectionFactory connectionFactory)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new DbContext(connectionFactory);
                }
            }
        }
        return _instance;
    }

    /// <summary>
    /// Gets or creates the current connection.
    /// </summary>
    public IDbConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        return _connection;
    }

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = GetConnection().BeginTransaction(isolationLevel);
        return _transaction;
    }

    /// <summary>
    /// Gets the current transaction, if any.
    /// </summary>
    public IDbTransaction? CurrentTransaction => _transaction;

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public void Commit()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public void Rollback()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }

    /// <summary>
    /// Creates a new command associated with the current connection and transaction.
    /// </summary>
    public IDbCommand CreateCommand()
    {
        var command = GetConnection().CreateCommand();
        command.Transaction = _transaction;
        return command;
    }

    /// <summary>
    /// Executes a non-query SQL command.
    /// </summary>
    public int ExecuteNonQuery(string sql, params (string name, object? value)[] parameters)
    {
        using var command = CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a scalar SQL command.
    /// </summary>
    public object? ExecuteScalar(string sql, params (string name, object? value)[] parameters)
    {
        using var command = CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        return command.ExecuteScalar();
    }

    /// <summary>
    /// Executes a query and returns a data reader.
    /// </summary>
    public IDataReader ExecuteReader(string sql, params (string name, object? value)[] parameters)
    {
        var command = CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        return command.ExecuteReader();
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Resets the singleton instance. Useful for testing.
    /// </summary>
    public static void ResetInstance()
    {
        lock (_lock)
        {
            _instance?.Dispose();
            _instance = null;
        }
    }
}
