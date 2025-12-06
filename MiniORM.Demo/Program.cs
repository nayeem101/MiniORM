using MiniORM.Core;
using MiniORM.Core.Connection;
using MiniORM.Core.Repository;
using MiniORM.Core.UnitOfWork;
using MiniORM.Demo.Entities;

namespace MiniORM.Demo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       MiniORM Demo - A Simple C# ORM Implementation          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Create in-memory SQLite database
        var connectionFactory = SqliteConnectionFactory.CreateInMemory();
        using var context = new DbContext(connectionFactory);

        // Create tables
        CreateTables(context);
        Console.WriteLine("✓ Database tables created successfully\n");

        // Demo 1: Basic Repository Pattern
        Console.WriteLine("═══ Demo 1: Basic Repository Pattern ═══");
        DemoBasicRepository(context);

        // Demo 2: Unit of Work Pattern
        Console.WriteLine("\n═══ Demo 2: Unit of Work Pattern ═══");
        DemoUnitOfWork(context);

        // Demo 3: LINQ-like Queries
        Console.WriteLine("\n═══ Demo 3: LINQ-like Queries ═══");
        DemoLinqQueries(context);

        // Demo 4: Change Tracking
        Console.WriteLine("\n═══ Demo 4: Change Tracking ═══");
        DemoChangeTracking(context);

        // Demo 5: Transactions
        Console.WriteLine("\n═══ Demo 5: Transactions ═══");
        DemoTransactions(context);

        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    Demo Complete!                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    static void CreateTables(DbContext context)
    {
        // Create Customers table
        context.ExecuteNonQuery(@"
            CREATE TABLE Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                Email TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                IsActive INTEGER NOT NULL
            )");

        // Create Orders table
        context.ExecuteNonQuery(@"
            CREATE TABLE Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER NOT NULL,
                ProductName TEXT NOT NULL,
                Total REAL NOT NULL,
                OrderDate TEXT NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            )");
    }

    static void DemoBasicRepository(DbContext context)
    {
        var repo = new Repository<Customer>(context);

        // Insert
        var customer1 = new Customer
        {
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.Now,
            IsActive = true
        };
        repo.Add(customer1);
        Console.WriteLine($"  ✓ Inserted customer with ID: {customer1.Id}");

        var customer2 = new Customer
        {
            Name = "Jane Smith",
            Email = "jane@example.com",
            CreatedAt = DateTime.Now,
            IsActive = true
        };
        repo.Add(customer2);
        Console.WriteLine($"  ✓ Inserted customer with ID: {customer2.Id}");

        // GetById
        var retrieved = repo.GetById(1);
        Console.WriteLine($"  ✓ Retrieved customer: {retrieved?.DisplayName}");

        // GetAll
        var allCustomers = repo.GetAll().ToList();
        Console.WriteLine($"  ✓ Total customers: {allCustomers.Count}");

        // Update
        if (retrieved != null)
        {
            retrieved.Email = "john.doe@newmail.com";
            repo.Update(retrieved);
            Console.WriteLine($"  ✓ Updated customer email");
        }

        // Count
        var count = repo.Count();
        Console.WriteLine($"  ✓ Customer count: {count}");
    }

    static void DemoUnitOfWork(DbContext context)
    {
        using var unitOfWork = new UnitOfWork(context);
        var customerRepo = unitOfWork.Repository<Customer>();
        var orderRepo = unitOfWork.Repository<Order>();

        // Add a new customer
        var customer = new Customer
        {
            Name = "Bob Wilson",
            Email = "bob@example.com",
            CreatedAt = DateTime.Now,
            IsActive = true
        };
        customerRepo.Add(customer);

        // Save to get the ID
        unitOfWork.SaveChanges();
        Console.WriteLine($"  ✓ Added customer via UoW with ID: {customer.Id}");

        // Add orders for this customer
        var order1 = new Order
        {
            CustomerId = customer.Id,
            ProductName = "Laptop",
            Total = 999.99m,
            OrderDate = DateTime.Now
        };
        orderRepo.Add(order1);

        var order2 = new Order
        {
            CustomerId = customer.Id,
            ProductName = "Mouse",
            Total = 29.99m,
            OrderDate = DateTime.Now
        };
        orderRepo.Add(order2);

        // Save all changes at once
        var affected = unitOfWork.SaveChanges();
        Console.WriteLine($"  ✓ Saved {affected} orders in single SaveChanges()");

        // Query orders
        var orders = orderRepo.GetAll().ToList();
        Console.WriteLine($"  ✓ Total orders: {orders.Count}");
    }

    static void DemoLinqQueries(DbContext context)
    {
        var repo = new Repository<Customer>(context);

        // Find with Contains (LIKE)
        var searchResults = repo.Find(c => c.Name.Contains("o")).ToList();
        Console.WriteLine($"  ✓ Customers with 'o' in name: {searchResults.Count}");
        foreach (var c in searchResults)
        {
            Console.WriteLine($"    - {c.Name}");
        }

        // Find with comparison
        var activeCustomers = repo.Find(c => c.IsActive == true).ToList();
        Console.WriteLine($"  ✓ Active customers: {activeCustomers.Count}");

        // FirstOrDefault
        var john = repo.FirstOrDefault(c => c.Name == "John Doe");
        Console.WriteLine($"  ✓ Found John Doe: {john != null}");

        // Any
        var hasJane = repo.Any(c => c.Email.Contains("jane"));
        Console.WriteLine($"  ✓ Has customer with 'jane' in email: {hasJane}");

        // Count with predicate
        var activeCount = repo.Count(c => c.IsActive == true);
        Console.WriteLine($"  ✓ Active customer count: {activeCount}");
    }

    static void DemoChangeTracking(DbContext context)
    {
        using var unitOfWork = new UnitOfWork(context);
        var repo = unitOfWork.Repository<Customer>();

        // Get a customer (will be tracked)
        var customer = repo.FirstOrDefault(c => c.Name == "John Doe");
        if (customer != null)
        {
            Console.WriteLine($"  ✓ Customer state before change: {customer.State}");

            // Modify the customer
            customer.Name = "John D. Doe";

            Console.WriteLine($"  ✓ Customer state after change: {customer.State}");

            // Save changes
            unitOfWork.SaveChanges();
            Console.WriteLine($"  ✓ Customer state after save: {customer.State}");
        }

        // Verify the change
        var updated = repo.FirstOrDefault(c => c.Name == "John D. Doe");
        Console.WriteLine($"  ✓ Updated name verified: {updated?.Name}");
    }

    static void DemoTransactions(DbContext context)
    {
        using var unitOfWork = new UnitOfWork(context);
        var repo = unitOfWork.Repository<Customer>();

        unitOfWork.BeginTransaction();
        try
        {
            var customer = new Customer
            {
                Name = "Transaction Test",
                Email = "transaction@test.com",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            repo.Add(customer);
            unitOfWork.SaveChanges();

            Console.WriteLine($"  ✓ Added customer in transaction with ID: {customer.Id}");

            // Simulate success - commit
            unitOfWork.Commit();
            Console.WriteLine("  ✓ Transaction committed successfully");
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            Console.WriteLine($"  ✗ Transaction rolled back: {ex.Message}");
        }

        // Verify transaction result
        var total = repo.Count();
        Console.WriteLine($"  ✓ Total customers after transaction: {total}");
    }
}
