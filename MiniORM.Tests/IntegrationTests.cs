using MiniORM.Core;
using MiniORM.Core.Repository;
using MiniORM.Core.UnitOfWork;

namespace MiniORM.Tests;

/// <summary>
/// Integration tests that verify end-to-end scenarios.
/// </summary>
public class IntegrationTests : DatabaseTestBase
{
    #region Unit of Work Integration Tests

    [Fact]
    public void UnitOfWork_AddAndSaveChanges_PersistsEntity()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();

        // Act
        var customer = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer);
        var affected = unitOfWork.SaveChanges();

        // Assert
        Assert.Equal(1, affected);
        Assert.True(customer.Id > 0);

        // Verify persistence
        var saved = repo.GetById(customer.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public void UnitOfWork_ModifyAndSaveChanges_UpdatesEntity()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();
        var customer = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer);
        unitOfWork.SaveChanges();

        // Act
        customer.Name = "John Updated";
        customer.Email = "john.updated@test.com";
        unitOfWork.SaveChanges();

        // Assert
        var updated = repo.GetById(customer.Id);
        Assert.Equal("John Updated", updated!.Name);
        Assert.Equal("john.updated@test.com", updated.Email);
    }

    [Fact]
    public void UnitOfWork_DeleteAndSaveChanges_RemovesEntity()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();
        var customer = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer);
        unitOfWork.SaveChanges();
        var id = customer.Id;

        // Act
        repo.Delete(customer);
        unitOfWork.SaveChanges();

        // Assert
        var deleted = repo.GetById(id);
        Assert.Null(deleted);
    }

    [Fact]
    public void UnitOfWork_MultipleSaveChanges_WorksCorrectly()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();

        // First save
        var customer1 = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer1);
        unitOfWork.SaveChanges();

        // Second save
        var customer2 = CreateTestCustomer("Jane", "jane@test.com");
        repo.Add(customer2);
        unitOfWork.SaveChanges();

        // Assert
        Assert.Equal(2, repo.Count());
    }

    [Fact]
    public void UnitOfWork_MixedOperationsInSingleSave_AllPersisted()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();

        // Setup initial data
        var existing = CreateTestCustomer("Existing", "existing@test.com");
        repo.Add(existing);
        unitOfWork.SaveChanges();

        // Act - Mix of add, update, delete
        var newCustomer = CreateTestCustomer("New", "new@test.com");
        repo.Add(newCustomer);

        existing.Name = "Updated";

        unitOfWork.SaveChanges();

        // Assert
        Assert.Equal(2, repo.Count());
        var updated = repo.GetById(existing.Id);
        Assert.Equal("Updated", updated!.Name);
    }

    #endregion

    #region Transaction Integration Tests

    [Fact]
    public void Transaction_Commit_PersistsData()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();

        // Act
        unitOfWork.BeginTransaction();
        var customer = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer);
        unitOfWork.SaveChanges();
        unitOfWork.Commit();

        // Assert
        var found = repo.GetById(customer.Id);
        Assert.NotNull(found);
    }

    [Fact]
    public void Transaction_Rollback_RevertsData()
    {
        // Arrange
        var directRepo = new Repository<TestCustomer>(Context);
        var existing = CreateTestCustomer("Existing", "existing@test.com");
        directRepo.Add(existing);
        var initialCount = directRepo.Count();

        // Act
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();
        unitOfWork.BeginTransaction();
        var newCustomer = CreateTestCustomer("New", "new@test.com");
        repo.Add(newCustomer);
        unitOfWork.SaveChanges();
        unitOfWork.Rollback();

        // Assert - Count should still be 1
        var finalCount = directRepo.Count();
        Assert.Equal(initialCount, finalCount);
    }

    #endregion

    #region LINQ Query Integration Tests

    [Fact]
    public void Find_ComplexPredicate_ReturnsCorrectResults()
    {
        // Arrange
        var repo = new Repository<TestCustomer>(Context);
        
        var john = CreateTestCustomer("John Doe", "john@test.com");
        john.Age = 25;
        john.IsActive = true;
        repo.Add(john);

        var jane = CreateTestCustomer("Jane Doe", "jane@test.com");
        jane.Age = 35;
        jane.IsActive = true;
        repo.Add(jane);

        var bob = CreateTestCustomer("Bob Smith", "bob@test.com");
        bob.Age = 20;
        bob.IsActive = false;
        repo.Add(bob);

        // Act - Find active customers with age > 20
        var results = repo.Find(c => c.IsActive == true && c.Age > 20).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, c => c.Name == "John Doe");
        Assert.Contains(results, c => c.Name == "Jane Doe");
    }

    [Fact]
    public void Find_StringContains_FiltersCorrectly()
    {
        // Arrange
        var repo = new Repository<TestCustomer>(Context);
        repo.Add(CreateTestCustomer("John Doe", "john@company.com"));
        repo.Add(CreateTestCustomer("Jane Doe", "jane@company.com"));
        repo.Add(CreateTestCustomer("Bob Smith", "bob@other.com"));

        // Act
        var results = repo.Find(c => c.Email.Contains("company")).ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    #endregion

    #region Change Tracking Integration Tests

    [Fact]
    public void ChangeTracking_AutomaticModificationDetection_Works()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(Context);
        var repo = unitOfWork.Repository<TestCustomer>();
        var customer = CreateTestCustomer("John", "john@test.com");
        repo.Add(customer);
        unitOfWork.SaveChanges();

        // Act - Modify and verify state change
        Assert.Equal(EntityState.Unchanged, customer.State);
        customer.Name = "John Updated";
        Assert.Equal(EntityState.Modified, customer.State);

        // Save and verify reset
        unitOfWork.SaveChanges();
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    [Fact]
    public void TrackedRepository_AutomaticallyTracksQueriedEntities()
    {
        // Arrange
        var directRepo = new Repository<TestCustomer>(Context);
        directRepo.Add(CreateTestCustomer("John", "john@test.com"));

        using var unitOfWork = new UnitOfWork(Context);
        var trackedRepo = unitOfWork.Repository<TestCustomer>();

        // Act
        var customer = trackedRepo.FirstOrDefault(c => c.Name == "John");
        Assert.NotNull(customer);

        // Verify tracked
        Assert.True(unitOfWork.ChangeTracker.IsTracking(customer));
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    #endregion

    #region Entity Mapping Integration Tests

    [Fact]
    public void EntityMapper_CorrectlyMapsAllTypes()
    {
        // Arrange
        var repo = new Repository<TestCustomer>(Context);
        var customer = new TestCustomer
        {
            Name = "John",
            Email = "john@test.com",
            Age = 30,
            Balance = 1234.56m,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0),
            IsActive = true
        };

        // Act
        repo.Add(customer);
        var loaded = repo.GetById(customer.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("John", loaded.Name);
        Assert.Equal("john@test.com", loaded.Email);
        Assert.Equal(30, loaded.Age);
        Assert.Equal(1234.56m, loaded.Balance);
        Assert.True(loaded.IsActive);
    }

    [Fact]
    public void EntityMapper_NotMappedPropertiesNotPersisted()
    {
        // Arrange
        var repo = new Repository<TestCustomer>(Context);
        var customer = CreateTestCustomer("John", "john@test.com");

        // Act
        repo.Add(customer);
        var loaded = repo.GetById(customer.Id);

        // Assert - DisplayName is computed from Name and Email
        Assert.NotNull(loaded);
        Assert.Equal($"{loaded.Name} <{loaded.Email}>", loaded.DisplayName);
    }

    #endregion

    #region Relationship Integration Tests

    [Fact]
    public void ForeignKey_InsertWithRelationship_Works()
    {
        // Arrange
        var customerRepo = new Repository<TestCustomer>(Context);
        var orderRepo = new Repository<TestOrder>(Context);

        var customer = CreateTestCustomer("John", "john@test.com");
        customerRepo.Add(customer);

        // Act
        var order = new TestOrder
        {
            CustomerId = customer.Id,
            Product = "Laptop",
            Total = 999.99m
        };
        orderRepo.Add(order);

        // Assert
        var savedOrder = orderRepo.GetById(order.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(customer.Id, savedOrder.CustomerId);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public void ConcurrentRepositories_SameContext_ShareConnection()
    {
        // Arrange
        var repo1 = new Repository<TestCustomer>(Context);
        var repo2 = new Repository<TestOrder>(Context);

        // Act
        var customer = CreateTestCustomer("John", "john@test.com");
        repo1.Add(customer);

        var order = new TestOrder
        {
            CustomerId = customer.Id,
            Product = "Test Product",
            Total = 50.00m
        };
        repo2.Add(order);

        // Assert
        Assert.Equal(1, repo1.Count());
        Assert.Equal(1, repo2.Count());
    }

    #endregion
}
