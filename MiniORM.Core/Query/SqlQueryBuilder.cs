using System.Text;

namespace MiniORM.Core.Query;

/// <summary>
/// Builds SQL SELECT queries using the Builder pattern with a fluent interface.
/// 
/// Design Pattern: Builder Pattern
/// - Separates construction of complex objects from their representation
/// 
/// Design Pattern: Fluent Interface
/// - Methods return 'this' to allow method chaining
/// </summary>
public class SqlQueryBuilder
{
    private readonly StringBuilder _selectClause = new StringBuilder();
    private readonly StringBuilder _fromClause = new StringBuilder();
    private readonly StringBuilder _whereClause = new StringBuilder();
    private readonly StringBuilder _joinClause = new StringBuilder();
    private readonly StringBuilder _orderByClause = new StringBuilder();
    private readonly StringBuilder _groupByClause = new StringBuilder();

    private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
    private int _parameterIndex = 0;

    private int? _top;
    private int? _skip;
    private int? _take;
    private bool _distinct;

    /// <summary>
    /// Gets all parameters added to this query.
    /// </summary>
    public IReadOnlyList<QueryParameter> Parameters => _parameters;

    #region SELECT Clause

    /// <summary>
    /// Starts building a SELECT query with specified columns.
    /// </summary>
    public SqlQueryBuilder Select(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
        {
            _selectClause.Append("*");
        }
        else
        {
            _selectClause.Append(string.Join(", ", columns.Select(c => $"[{c}]")));
        }
        return this;
    }

    /// <summary>
    /// Selects all columns.
    /// </summary>
    public SqlQueryBuilder SelectAll()
    {
        _selectClause.Append("*");
        return this;
    }

    /// <summary>
    /// Selects specific columns from a typed entity.
    /// </summary>
    public SqlQueryBuilder Select<TEntity>() where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        var columns = metadata.MappedProperties.Select(p => $"[{p.ColumnName}]");
        _selectClause.Append(string.Join(", ", columns));
        return this;
    }

    /// <summary>
    /// Adds DISTINCT to the query.
    /// </summary>
    public SqlQueryBuilder Distinct()
    {
        _distinct = true;
        return this;
    }

    /// <summary>
    /// Limits the number of rows returned (SQL Server TOP).
    /// </summary>
    public SqlQueryBuilder Top(int count)
    {
        _top = count;
        return this;
    }

    /// <summary>
    /// Sets the number of rows to skip (OFFSET).
    /// </summary>
    public SqlQueryBuilder Skip(int count)
    {
        _skip = count;
        return this;
    }

    /// <summary>
    /// Sets the number of rows to take (LIMIT/FETCH).
    /// </summary>
    public SqlQueryBuilder Take(int count)
    {
        _take = count;
        return this;
    }

    #endregion

    #region FROM Clause

    /// <summary>
    /// Specifies the table to select from.
    /// </summary>
    public SqlQueryBuilder From(string tableName, string? alias = null)
    {
        _fromClause.Append($"[{tableName}]");
        if (!string.IsNullOrEmpty(alias))
        {
            _fromClause.Append($" AS [{alias}]");
        }
        return this;
    }

    /// <summary>
    /// Specifies the table to select from using entity metadata.
    /// </summary>
    public SqlQueryBuilder From<TEntity>(string? alias = null) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        return From(metadata.TableName, alias);
    }

    #endregion

    #region WHERE Clause

    /// <summary>
    /// Adds a WHERE condition.
    /// </summary>
    public SqlQueryBuilder Where(string condition)
    {
        if (_whereClause.Length > 0)
        {
            _whereClause.Append(" AND ");
        }
        _whereClause.Append($"({condition})");
        return this;
    }

    /// <summary>
    /// Adds a WHERE condition with a parameterized value.
    /// </summary>
    public SqlQueryBuilder Where(string column, object? value)
    {
        var paramName = AddParameter(value);
        return Where($"[{column}] = {paramName}");
    }

    /// <summary>
    /// Adds a WHERE condition with a comparison operator.
    /// </summary>
    public SqlQueryBuilder Where(string column, string op, object? value)
    {
        var paramName = AddParameter(value);
        return Where($"[{column}] {op} {paramName}");
    }

    /// <summary>
    /// Adds an OR condition.
    /// </summary>
    public SqlQueryBuilder OrWhere(string condition)
    {
        if (_whereClause.Length > 0)
        {
            _whereClause.Append(" OR ");
        }
        _whereClause.Append($"({condition})");
        return this;
    }

    /// <summary>
    /// Adds a WHERE IN clause.
    /// </summary>
    public SqlQueryBuilder WhereIn<T>(string column, IEnumerable<T> values)
    {
        var paramNames = values.Select(v => AddParameter(v)).ToList();
        if (paramNames.Any())
        {
            return Where($"[{column}] IN ({string.Join(", ", paramNames)})");
        }
        return this;
    }

    /// <summary>
    /// Adds a WHERE LIKE clause.
    /// </summary>
    public SqlQueryBuilder WhereLike(string column, string pattern)
    {
        var paramName = AddParameter(pattern);
        return Where($"[{column}] LIKE {paramName}");
    }

    /// <summary>
    /// Adds a WHERE IS NULL clause.
    /// </summary>
    public SqlQueryBuilder WhereNull(string column)
    {
        return Where($"[{column}] IS NULL");
    }

    /// <summary>
    /// Adds a WHERE IS NOT NULL clause.
    /// </summary>
    public SqlQueryBuilder WhereNotNull(string column)
    {
        return Where($"[{column}] IS NOT NULL");
    }

    #endregion

    #region JOIN Clause

    /// <summary>
    /// Adds an INNER JOIN.
    /// </summary>
    public SqlQueryBuilder Join(string table, string condition, string? alias = null)
    {
        _joinClause.Append($" INNER JOIN [{table}]");
        if (!string.IsNullOrEmpty(alias))
        {
            _joinClause.Append($" AS [{alias}]");
        }
        _joinClause.Append($" ON {condition}");
        return this;
    }

    /// <summary>
    /// Adds a LEFT JOIN.
    /// </summary>
    public SqlQueryBuilder LeftJoin(string table, string condition, string? alias = null)
    {
        _joinClause.Append($" LEFT JOIN [{table}]");
        if (!string.IsNullOrEmpty(alias))
        {
            _joinClause.Append($" AS [{alias}]");
        }
        _joinClause.Append($" ON {condition}");
        return this;
    }

    #endregion

    #region ORDER BY Clause

    /// <summary>
    /// Adds an ORDER BY clause.
    /// </summary>
    public SqlQueryBuilder OrderBy(string column, bool descending = false)
    {
        if (_orderByClause.Length > 0)
        {
            _orderByClause.Append(", ");
        }
        _orderByClause.Append($"[{column}]");
        if (descending)
        {
            _orderByClause.Append(" DESC");
        }
        return this;
    }

    /// <summary>
    /// Adds an ORDER BY DESC clause.
    /// </summary>
    public SqlQueryBuilder OrderByDescending(string column)
    {
        return OrderBy(column, true);
    }

    #endregion

    #region GROUP BY Clause

    /// <summary>
    /// Adds a GROUP BY clause.
    /// </summary>
    public SqlQueryBuilder GroupBy(params string[] columns)
    {
        _groupByClause.Append(string.Join(", ", columns.Select(c => $"[{c}]")));
        return this;
    }

    #endregion

    #region Parameters

    /// <summary>
    /// Adds a parameter and returns its name.
    /// </summary>
    public string AddParameter(object? value)
    {
        var name = $"@p{_parameterIndex++}";
        _parameters.Add(new QueryParameter(name, value));
        return name;
    }

    /// <summary>
    /// Gets parameters as tuples for DbContext execution.
    /// </summary>
    public (string name, object? value)[] GetParameterTuples()
    {
        return _parameters.Select(p => (p.Name, p.Value)).ToArray();
    }

    #endregion

    #region Build Methods

    /// <summary>
    /// Builds the complete SELECT query.
    /// </summary>
    public string Build()
    {
        var sql = new StringBuilder("SELECT ");

        if (_distinct)
        {
            sql.Append("DISTINCT ");
        }

        if (_top.HasValue && !_take.HasValue)
        {
            sql.Append($"TOP {_top.Value} ");
        }

        sql.Append(_selectClause.Length > 0 ? _selectClause.ToString() : "*");
        sql.Append(" FROM ");
        sql.Append(_fromClause);
        sql.Append(_joinClause);

        if (_whereClause.Length > 0)
        {
            sql.Append(" WHERE ");
            sql.Append(_whereClause);
        }

        if (_groupByClause.Length > 0)
        {
            sql.Append(" GROUP BY ");
            sql.Append(_groupByClause);
        }

        if (_orderByClause.Length > 0)
        {
            sql.Append(" ORDER BY ");
            sql.Append(_orderByClause);
        }

        // SQLite uses LIMIT/OFFSET
        if (_take.HasValue)
        {
            sql.Append($" LIMIT {_take.Value}");
        }
        if (_skip.HasValue)
        {
            sql.Append($" OFFSET {_skip.Value}");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Builds a COUNT query.
    /// </summary>
    public string BuildCount()
    {
        var sql = new StringBuilder("SELECT COUNT(*) FROM ");
        sql.Append(_fromClause);
        sql.Append(_joinClause);

        if (_whereClause.Length > 0)
        {
            sql.Append(" WHERE ");
            sql.Append(_whereClause);
        }

        return sql.ToString();
    }

    #endregion
}
