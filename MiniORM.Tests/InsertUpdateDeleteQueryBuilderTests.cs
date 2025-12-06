using MiniORM.Core.Query;

namespace MiniORM.Tests;

/// <summary>
/// Tests for InsertQueryBuilder, UpdateQueryBuilder, and DeleteQueryBuilder.
/// </summary>
public class InsertUpdateDeleteQueryBuilderTests
{
    #region INSERT Tests

    [Fact]
    public void InsertBuild_WithValues_GeneratesInsertStatement()
    {
        // Arrange
        var builder = new InsertQueryBuilder()
            .Into("Customers")
            .Value("Name", "John")
            .Value("Email", "john@test.com");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("INSERT INTO [Customers]", sql);
        Assert.Contains("[Name]", sql);
        Assert.Contains("[Email]", sql);
        Assert.Contains("VALUES", sql);
        Assert.Contains("@p0", sql);
        Assert.Contains("@p1", sql);
    }

    [Fact]
    public void InsertBuild_WithEntity_GeneratesInsertFromMetadata()
    {
        // Arrange
        var customer = new TestCustomer
        {
            Name = "John",
            Email = "john@test.com",
            Age = 30,
            Balance = 100.00m,
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        var builder = new InsertQueryBuilder()
            .Into<TestCustomer>()
            .Values(customer);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("INSERT INTO [TestCustomers]", sql);
        Assert.Contains("[CustomerName]", sql);  // Uses column attribute
        Assert.DoesNotContain("[Id]", sql);      // Excludes auto-generated PK
    }

    [Fact]
    public void InsertBuild_WithReturnId_GeneratesSelectLastInsertRowid()
    {
        // Arrange
        var builder = new InsertQueryBuilder()
            .Into("Customers")
            .Value("Name", "John")
            .ReturnId(true);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("SELECT last_insert_rowid()", sql);
    }

    [Fact]
    public void InsertBuild_WithoutReturnId_NoSelectStatement()
    {
        // Arrange
        var builder = new InsertQueryBuilder()
            .Into("Customers")
            .Value("Name", "John")
            .ReturnId(false);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.DoesNotContain("SELECT", sql);
    }

    [Fact]
    public void InsertGetParameterTuples_ReturnsCorrectValues()
    {
        // Arrange
        var builder = new InsertQueryBuilder()
            .Into("Customers")
            .Value("Name", "John")
            .Value("Age", 25);

        // Act
        var tuples = builder.GetParameterTuples();

        // Assert
        Assert.Equal(2, tuples.Length);
        Assert.Equal("John", tuples[0].value);
        Assert.Equal(25, tuples[1].value);
    }

    #endregion

    #region UPDATE Tests

    [Fact]
    public void UpdateBuild_WithSetAndWhere_GeneratesUpdateStatement()
    {
        // Arrange
        var builder = new UpdateQueryBuilder()
            .Table("Customers")
            .Set("Name", "John Updated")
            .Where("Id", 1);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("UPDATE [Customers] SET", sql);
        Assert.Contains("[Name] = @p0", sql);
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Id] = @p1", sql);
    }

    [Fact]
    public void UpdateBuild_WithMultipleSets_GeneratesAllSetClauses()
    {
        // Arrange
        var builder = new UpdateQueryBuilder()
            .Table("Customers")
            .Set("Name", "John")
            .Set("Email", "john@new.com")
            .Set("Age", 30)
            .Where("Id", 1);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[Name] = @p0", sql);
        Assert.Contains("[Email] = @p1", sql);
        Assert.Contains("[Age] = @p2", sql);
    }

    [Fact]
    public void UpdateBuild_WithoutWhere_ThrowsException()
    {
        // Arrange
        var builder = new UpdateQueryBuilder()
            .Table("Customers")
            .Set("Name", "John");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("WHERE", exception.Message);
    }

    [Fact]
    public void UpdateBuild_WithEntity_GeneratesAllUpdateableFields()
    {
        // Arrange
        var customer = new TestCustomer
        {
            Id = 1,
            Name = "John Updated",
            Email = "john@new.com",
            Age = 31,
            Balance = 150.00m,
            CreatedAt = DateTime.Now,
            IsActive = false
        };

        var builder = new UpdateQueryBuilder()
            .Table<TestCustomer>()
            .SetAll(customer)
            .WherePrimaryKey(customer);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("UPDATE [TestCustomers] SET", sql);
        Assert.Contains("[CustomerName]", sql);
        Assert.DoesNotContain("[Id] =", sql.Split("WHERE")[0]); // Id not in SET clause
    }

    [Fact]
    public void UpdateWherePrimaryKey_UsesCorrectPrimaryKeyColumn()
    {
        // Arrange
        var customer = new TestCustomer { Id = 42, Name = "Test" };
        var builder = new UpdateQueryBuilder()
            .Table<TestCustomer>()
            .Set("CustomerName", "Updated")
            .WherePrimaryKey(customer);

        // Act
        var sql = builder.Build();
        var tuples = builder.GetParameterTuples();

        // Assert
        Assert.Contains("[Id] = @p1", sql);
        Assert.Equal(42, tuples[1].value);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public void DeleteBuild_WithWhere_GeneratesDeleteStatement()
    {
        // Arrange
        var builder = new DeleteQueryBuilder()
            .From("Customers")
            .Where("Id", 1);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Equal("DELETE FROM [Customers] WHERE ([Id] = @p0)", sql);
    }

    [Fact]
    public void DeleteBuild_WithoutWhere_ThrowsException()
    {
        // Arrange
        var builder = new DeleteQueryBuilder()
            .From("Customers");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("WHERE", exception.Message);
    }

    [Fact]
    public void DeleteBuild_FromEntity_UsesTableName()
    {
        // Arrange
        var builder = new DeleteQueryBuilder()
            .From<TestCustomer>()
            .Where("Id", 1);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("DELETE FROM [TestCustomers]", sql);
    }

    [Fact]
    public void DeleteWherePrimaryKey_UsesEntityPrimaryKeyValue()
    {
        // Arrange
        var customer = new TestCustomer { Id = 99, Name = "ToDelete" };
        var builder = new DeleteQueryBuilder()
            .From<TestCustomer>()
            .WherePrimaryKey(customer);

        // Act
        var sql = builder.Build();
        var tuples = builder.GetParameterTuples();

        // Assert
        Assert.Contains("[Id] = @p0", sql);
        Assert.Equal(99, tuples[0].value);
    }

    [Fact]
    public void DeleteWherePrimaryKeyById_UsesProvidedId()
    {
        // Arrange
        var builder = new DeleteQueryBuilder()
            .From<TestCustomer>()
            .WherePrimaryKey<TestCustomer, int>(123);

        // Act
        var sql = builder.Build();
        var tuples = builder.GetParameterTuples();

        // Assert
        Assert.Contains("[Id] = @p0", sql);
        Assert.Equal(123, tuples[0].value);
    }

    #endregion
}
