using MiniORM.Core;
using MiniORM.Core.Connection;

namespace MiniORM.Tests;

/// <summary>
/// Base class for tests that need database access.
/// Provides a fresh in-memory SQLite database for each test.
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    protected DbContext Context { get; }
    protected SqliteConnectionFactory ConnectionFactory { get; }

    protected DatabaseTestBase()
    {
        // Create in-memory SQLite database
        // Using shared cache so connection persists across operations
        ConnectionFactory = new SqliteConnectionFactory("Data Source=:memory:;Cache=Shared");
        Context = new DbContext(ConnectionFactory);

        // Create test tables
        CreateTestTables();
    }

    private void CreateTestTables()
    {
        Context.ExecuteNonQuery(@"
            CREATE TABLE TestCustomers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                Email TEXT NOT NULL,
                Age INTEGER NOT NULL,
                Balance REAL NOT NULL,
                CreatedAt TEXT NOT NULL,
                IsActive INTEGER NOT NULL
            )");

        Context.ExecuteNonQuery(@"
            CREATE TABLE TestOrders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER NOT NULL,
                Product TEXT NOT NULL,
                Total REAL NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES TestCustomers(Id)
            )");
    }

    protected TestCustomer CreateTestCustomer(string name = "Test User", string email = "test@example.com")
    {
        return new TestCustomer
        {
            Name = name,
            Email = email,
            Age = 25,
            Balance = 100.00m,
            CreatedAt = DateTime.Now,
            IsActive = true
        };
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
