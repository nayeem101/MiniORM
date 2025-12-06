using MiniORM.Core;
using MiniORM.Core.Repository;

namespace MiniORM.Tests;

/// <summary>
/// Tests for Repository class.
/// </summary>
public class RepositoryTests : DatabaseTestBase
{
    private readonly Repository<TestCustomer> _repository;

    public RepositoryTests()
    {
        _repository = new Repository<TestCustomer>(Context);
    }

    #region GetById Tests

    [Fact]
    public void GetById_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");
        _repository.Add(customer);

        // Act
        var result = _repository.GetById(customer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public void GetById_NonExistingEntity_ReturnsNull()
    {
        // Act
        var result = _repository.GetById(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_WithEntities_ReturnsAllEntities()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));
        _repository.Add(CreateTestCustomer("Jane", "jane@test.com"));
        _repository.Add(CreateTestCustomer("Bob", "bob@test.com"));

        // Act
        var results = _repository.GetAll().ToList();

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void GetAll_EmptyTable_ReturnsEmptyList()
    {
        // Act
        var results = _repository.GetAll().ToList();

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region Add Tests

    [Fact]
    public void Add_NewEntity_ReturnsEntityWithId()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");

        // Act
        var result = _repository.Add(customer);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal(customer.Name, result.Name);
    }

    [Fact]
    public void Add_MultipleEntities_GeneratesUniqueIds()
    {
        // Arrange & Act
        var customer1 = _repository.Add(CreateTestCustomer("John", "john@test.com"));
        var customer2 = _repository.Add(CreateTestCustomer("Jane", "jane@test.com"));

        // Assert
        Assert.NotEqual(customer1.Id, customer2.Id);
    }

    [Fact]
    public void Add_SetsEntityStateToUnchanged()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");

        // Act
        _repository.Add(customer);

        // Assert
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_ExistingEntity_UpdatesDatabase()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");
        _repository.Add(customer);
        customer.Name = "John Updated";
        customer.Email = "john.updated@test.com";

        // Act
        _repository.Update(customer);

        // Assert
        var updated = _repository.GetById(customer.Id);
        Assert.Equal("John Updated", updated!.Name);
        Assert.Equal("john.updated@test.com", updated.Email);
    }

    [Fact]
    public void Update_SetsEntityStateToUnchanged()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");
        _repository.Add(customer);
        customer.Name = "John Updated";

        // Act
        _repository.Update(customer);

        // Assert
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");
        _repository.Add(customer);

        // Act
        _repository.Delete(customer);

        // Assert
        var deleted = _repository.GetById(customer.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public void DeleteById_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var customer = CreateTestCustomer("John", "john@test.com");
        _repository.Add(customer);
        var id = customer.Id;

        // Act
        _repository.Delete(id);

        // Assert
        var deleted = _repository.GetById(id);
        Assert.Null(deleted);
    }

    #endregion

    #region Find Tests

    [Fact]
    public void Find_WithMatchingPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));
        _repository.Add(CreateTestCustomer("Jane", "jane@test.com"));
        _repository.Add(CreateTestCustomer("Bob", "bob@test.com"));

        // Act
        var results = _repository.Find(c => c.Name.Contains("J")).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Contains("J", r.Name));
    }

    [Fact]
    public void Find_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));

        // Act
        var results = _repository.Find(c => c.Name == "Nobody").ToList();

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region FirstOrDefault Tests

    [Fact]
    public void FirstOrDefault_WithMatch_ReturnsFirstMatch()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));
        _repository.Add(CreateTestCustomer("Jane", "jane@test.com"));

        // Act
        var result = _repository.FirstOrDefault(c => c.Name == "Jane");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.Name);
    }

    [Fact]
    public void FirstOrDefault_WithNoMatch_ReturnsNull()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));

        // Act
        var result = _repository.FirstOrDefault(c => c.Name == "Nobody");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Count Tests

    [Fact]
    public void Count_WithEntities_ReturnsCorrectCount()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));
        _repository.Add(CreateTestCustomer("Jane", "jane@test.com"));
        _repository.Add(CreateTestCustomer("Bob", "bob@test.com"));

        // Act
        var count = _repository.Count();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void Count_WithPredicate_ReturnsFilteredCount()
    {
        // Arrange
        var john = CreateTestCustomer("John", "john@test.com");
        john.IsActive = true;
        _repository.Add(john);

        var jane = CreateTestCustomer("Jane", "jane@test.com");
        jane.IsActive = false;
        _repository.Add(jane);

        var bob = CreateTestCustomer("Bob", "bob@test.com");
        bob.IsActive = true;
        _repository.Add(bob);

        // Act
        var activeCount = _repository.Count(c => c.IsActive == true);

        // Assert
        Assert.Equal(2, activeCount);
    }

    [Fact]
    public void Count_EmptyTable_ReturnsZero()
    {
        // Act
        var count = _repository.Count();

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region Any Tests

    [Fact]
    public void Any_WithMatchingEntities_ReturnsTrue()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));

        // Act
        var exists = _repository.Any(c => c.Name == "John");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void Any_WithNoMatchingEntities_ReturnsFalse()
    {
        // Arrange
        _repository.Add(CreateTestCustomer("John", "john@test.com"));

        // Act
        var exists = _repository.Any(c => c.Name == "Nobody");

        // Assert
        Assert.False(exists);
    }

    #endregion
}
