using MiniORM.Core.Query;

namespace MiniORM.Tests;

/// <summary>
/// Tests for ExpressionParser (LINQ expression to SQL conversion).
/// </summary>
public class ExpressionParserTests
{
  private readonly ExpressionParser _parser = new();

  #region Equality Tests

  [Fact]
  public void Parse_EqualityComparison_GeneratesCorrectSql()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name == "John", query);

    // Assert
    Assert.Contains("[CustomerName] =", sql);
    Assert.Single(query.Parameters);
  }

  [Fact]
  public void Parse_InequalityComparison_GeneratesNotEqual()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Id != 5, query);

    // Assert
    Assert.Contains("[Id] <>", sql);
  }

  #endregion

  #region Comparison Operators Tests

  [Fact]
  public void Parse_GreaterThan_GeneratesCorrectOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Age > 18, query);

    // Assert
    Assert.Contains("[Age] >", sql);
  }

  [Fact]
  public void Parse_GreaterThanOrEqual_GeneratesCorrectOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Age >= 21, query);

    // Assert
    Assert.Contains("[Age] >=", sql);
  }

  [Fact]
  public void Parse_LessThan_GeneratesCorrectOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Balance < 100.00m, query);

    // Assert
    Assert.Contains("[Balance] <", sql);
  }

  [Fact]
  public void Parse_LessThanOrEqual_GeneratesCorrectOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Balance <= 50.00m, query);

    // Assert
    Assert.Contains("[Balance] <=", sql);
  }

  #endregion

  #region Boolean Tests

  [Fact]
  public void Parse_BooleanProperty_GeneratesCorrectSql()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.IsActive == true, query);

    // Assert
    Assert.Contains("[IsActive] =", sql);
  }

  #endregion

  #region String Method Tests

  [Fact]
  public void Parse_StringContains_GeneratesLikeWithWildcards()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name.Contains("test"), query);

    // Assert
    Assert.Contains("[CustomerName] LIKE", sql);
    Assert.Contains("%test%", query.Parameters[0].Value?.ToString());
  }

  [Fact]
  public void Parse_StringStartsWith_GeneratesLikeWithTrailingWildcard()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Email.StartsWith("john"), query);

    // Assert
    Assert.Contains("[Email] LIKE", sql);
    Assert.Equal("john%", query.Parameters[0].Value);
  }

  [Fact]
  public void Parse_StringEndsWith_GeneratesLikeWithLeadingWildcard()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Email.EndsWith(".com"), query);

    // Assert
    Assert.Contains("[Email] LIKE", sql);
    Assert.Equal("%.com", query.Parameters[0].Value);
  }

  [Fact]
  public void Parse_StringToUpper_GeneratesUpperFunction()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name.ToUpper() == "JOHN", query);

    // Assert
    Assert.Contains("UPPER([CustomerName])", sql);
  }

  [Fact]
  public void Parse_StringToLower_GeneratesLowerFunction()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name.ToLower() == "john", query);

    // Assert
    Assert.Contains("LOWER([CustomerName])", sql);
  }

  #endregion

  #region Logical Operator Tests

  [Fact]
  public void Parse_AndCondition_GeneratesAndOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Age > 18 && c.IsActive == true, query);

    // Assert
    Assert.Contains("AND", sql);
    Assert.Contains("[Age] >", sql);
    Assert.Contains("[IsActive] =", sql);
  }

  [Fact]
  public void Parse_OrCondition_GeneratesOrOperator()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Age < 18 || c.Age > 65, query);

    // Assert
    Assert.Contains("OR", sql);
  }

  [Fact]
  public void Parse_ComplexCondition_MaintainsPrecedence()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c =>
        (c.Age >= 18 && c.Age <= 65) && c.IsActive == true, query);

    // Assert
    Assert.Contains("AND", sql);
    Assert.Contains("[Age] >=", sql);
    Assert.Contains("[Age] <=", sql);
    Assert.Contains("[IsActive]", sql);
  }

  #endregion

  #region Variable Capture Tests

  [Fact]
  public void Parse_CapturedVariable_EvaluatesValue()
  {
    // Arrange
    var query = new SqlQueryBuilder();
    var minAge = 21;

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Age >= minAge, query);

    // Assert
    Assert.Contains("[Age] >=", sql);
    Assert.Equal(21, query.Parameters[0].Value);
  }

  [Fact]
  public void Parse_CapturedStringVariable_EvaluatesValue()
  {
    // Arrange
    var query = new SqlQueryBuilder();
    var searchName = "John";

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name == searchName, query);

    // Assert
    Assert.Equal("John", query.Parameters[0].Value);
  }

  #endregion

  #region Column Mapping Tests

  [Fact]
  public void Parse_PropertyWithColumnAttribute_UsesColumnName()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => c.Name == "Test", query);

    // Assert
    Assert.Contains("[CustomerName]", sql);  // Uses Column attribute's name
    Assert.DoesNotContain("[Name]", sql);
  }

  #endregion

  #region Not Operator Tests

  [Fact]
  public void Parse_NotOperator_GeneratesNotClause()
  {
    // Arrange
    var query = new SqlQueryBuilder();

    // Act
    var sql = _parser.Parse<TestCustomer>(c => !(c.IsActive == true), query);

    // Assert
    Assert.Contains("NOT", sql);
  }

  #endregion
}
