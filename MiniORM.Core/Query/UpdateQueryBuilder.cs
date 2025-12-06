using System.Text;

namespace MiniORM.Core.Query;

/// <summary>
/// Builds SQL UPDATE queries.
/// </summary>
public class UpdateQueryBuilder
{
    private string _tableName = "";
    private readonly List<string> _setClauses = new List<string>();
    private readonly StringBuilder _whereClause = new StringBuilder();
    private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
    private int _parameterIndex = 0;

    /// <summary>
    /// Gets all parameters added to this query.
    /// </summary>
    public IReadOnlyList<QueryParameter> Parameters => _parameters;

    /// <summary>
    /// Sets the table to update.
    /// </summary>
    public UpdateQueryBuilder Table(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    /// <summary>
    /// Sets the table to update using entity metadata.
    /// </summary>
    public UpdateQueryBuilder Table<TEntity>() where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        _tableName = metadata.TableName;
        return this;
    }

    /// <summary>
    /// Adds a SET clause.
    /// </summary>
    public UpdateQueryBuilder Set(string column, object? value)
    {
        var paramName = AddParameter(value);
        _setClauses.Add($"[{column}] = {paramName}");
        return this;
    }

    /// <summary>
    /// Sets all mapped properties from an entity (excluding primary key).
    /// </summary>
    public UpdateQueryBuilder SetAll<TEntity>(TEntity entity) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        foreach (var prop in metadata.GetUpdateProperties())
        {
            Set(prop.ColumnName, prop.GetValue(entity));
        }
        return this;
    }

    /// <summary>
    /// Adds a WHERE condition.
    /// </summary>
    public UpdateQueryBuilder Where(string condition)
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
    public UpdateQueryBuilder Where(string column, object? value)
    {
        var paramName = AddParameter(value);
        return Where($"[{column}] = {paramName}");
    }

    /// <summary>
    /// Sets the WHERE clause for the primary key.
    /// </summary>
    public UpdateQueryBuilder WherePrimaryKey<TEntity>(TEntity entity) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        if (metadata.PrimaryKey != null)
        {
            Where(metadata.PrimaryKey.ColumnName, metadata.PrimaryKey.GetValue(entity));
        }
        return this;
    }

    private string AddParameter(object? value)
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

    /// <summary>
    /// Builds the UPDATE query.
    /// </summary>
    public string Build()
    {
        if (_whereClause.Length == 0)
        {
            throw new InvalidOperationException("UPDATE query must have a WHERE clause for safety.");
        }

        var sql = new StringBuilder();
        sql.Append($"UPDATE [{_tableName}] SET ");
        sql.Append(string.Join(", ", _setClauses));
        sql.Append(" WHERE ");
        sql.Append(_whereClause);

        return sql.ToString();
    }
}
