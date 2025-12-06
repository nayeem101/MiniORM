using MiniORM.Core;
using MiniORM.Core.Query;

namespace MiniORM.Tests;

/// <summary>
/// Tests for SqlQueryBuilder (SELECT queries).
/// </summary>
public class SqlQueryBuilderTests
{
    #region Basic SELECT Tests

    [Fact]
    public void Build_SelectAll_GeneratesSelectStar()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Equal("SELECT * FROM [Customers]", sql);
    }

    [Fact]
    public void Build_SelectColumns_GeneratesColumnList()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .Select("Name", "Email")
            .From("Customers");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Equal("SELECT [Name], [Email] FROM [Customers]", sql);
    }

    [Fact]
    public void Build_SelectFromEntity_GeneratesAllMappedColumns()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .Select<TestCustomer>()
            .From<TestCustomer>();

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[CustomerName]", sql);
        Assert.Contains("[Email]", sql);
        Assert.Contains("[Age]", sql);
        Assert.Contains("FROM [TestCustomers]", sql);
    }

    #endregion

    #region WHERE Clause Tests

    [Fact]
    public void Build_WithWhere_GeneratesWhereClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Where("Id", 1);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("WHERE ([Id] = @p0)", sql);
        Assert.Single(builder.Parameters);
        Assert.Equal(1, builder.Parameters[0].Value);
    }

    [Fact]
    public void Build_WithMultipleWhere_GeneratesAndConditions()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Where("IsActive", true)
            .Where("Age", ">", 18);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("WHERE", sql);
        Assert.Contains("[IsActive] = @p0", sql);
        Assert.Contains("AND", sql);
        Assert.Contains("[Age] > @p1", sql);
    }

    [Fact]
    public void Build_WithWhereIn_GeneratesInClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .WhereIn("Id", new[] { 1, 2, 3 });

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[Id] IN (@p0, @p1, @p2)", sql);
        Assert.Equal(3, builder.Parameters.Count);
    }

    [Fact]
    public void Build_WithWhereLike_GeneratesLikeClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .WhereLike("Name", "%John%");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[Name] LIKE @p0", sql);
        Assert.Equal("%John%", builder.Parameters[0].Value);
    }

    [Fact]
    public void Build_WithWhereNull_GeneratesIsNull()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .WhereNull("MiddleName");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[MiddleName] IS NULL", sql);
    }

    [Fact]
    public void Build_WithWhereNotNull_GeneratesIsNotNull()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .WhereNotNull("Email");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("[Email] IS NOT NULL", sql);
    }

    #endregion

    #region ORDER BY Tests

    [Fact]
    public void Build_WithOrderBy_GeneratesOrderByClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .OrderBy("Name");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("ORDER BY [Name]", sql);
    }

    [Fact]
    public void Build_WithOrderByDescending_GeneratesDescClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .OrderByDescending("CreatedAt");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("ORDER BY [CreatedAt] DESC", sql);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void Build_WithTake_GeneratesLimitClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Take(10);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("LIMIT 10", sql);
    }

    [Fact]
    public void Build_WithSkipAndTake_GeneratesLimitOffset()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Skip(20)
            .Take(10);

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("LIMIT 10", sql);
        Assert.Contains("OFFSET 20", sql);
    }

    #endregion

    #region DISTINCT Tests

    [Fact]
    public void Build_WithDistinct_GeneratesDistinctClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .Select("Name")
            .From("Customers")
            .Distinct();

        // Act
        var sql = builder.Build();

        // Assert
        Assert.StartsWith("SELECT DISTINCT", sql);
    }

    #endregion

    #region JOIN Tests

    [Fact]
    public void Build_WithJoin_GeneratesJoinClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Orders")
            .Join("Customers", "[Orders].[CustomerId] = [Customers].[Id]");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("INNER JOIN [Customers] ON [Orders].[CustomerId] = [Customers].[Id]", sql);
    }

    [Fact]
    public void Build_WithLeftJoin_GeneratesLeftJoinClause()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .LeftJoin("Orders", "[Customers].[Id] = [Orders].[CustomerId]");

        // Act
        var sql = builder.Build();

        // Assert
        Assert.Contains("LEFT JOIN [Orders] ON [Customers].[Id] = [Orders].[CustomerId]", sql);
    }

    #endregion

    #region COUNT Tests

    [Fact]
    public void BuildCount_GeneratesCountQuery()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .From("Customers")
            .Where("IsActive", true);

        // Act
        var sql = builder.BuildCount();

        // Assert
        Assert.StartsWith("SELECT COUNT(*) FROM [Customers]", sql);
        Assert.Contains("WHERE", sql);
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void AddParameter_CreatesUniqueParameterNames()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Where("Name", "John")
            .Where("Age", 25);

        // Act
        var params1 = builder.Parameters;

        // Assert
        Assert.Equal("@p0", params1[0].Name);
        Assert.Equal("@p1", params1[1].Name);
    }

    [Fact]
    public void GetParameterTuples_ReturnsCorrectFormat()
    {
        // Arrange
        var builder = new SqlQueryBuilder()
            .SelectAll()
            .From("Customers")
            .Where("Id", 42);

        // Act
        var tuples = builder.GetParameterTuples();

        // Assert
        Assert.Single(tuples);
        Assert.Equal("@p0", tuples[0].name);
        Assert.Equal(42, tuples[0].value);
    }

    #endregion
}
