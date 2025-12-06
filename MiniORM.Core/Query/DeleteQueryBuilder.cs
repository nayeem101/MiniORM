using System.Text;

namespace MiniORM.Core.Query;

/// <summary>
/// Builds SQL DELETE queries.
/// </summary>
public class DeleteQueryBuilder
{
    private string _tableName = "";
    private readonly StringBuilder _whereClause = new StringBuilder();
    private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
    private int _parameterIndex = 0;

    /// <summary>
    /// Gets all parameters added to this query.
    /// </summary>
    public IReadOnlyList<QueryParameter> Parameters => _parameters;

    /// <summary>
    /// Sets the table to delete from.
    /// </summary>
    public DeleteQueryBuilder From(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    /// <summary>
    /// Sets the table to delete from using entity metadata.
    /// </summary>
    public DeleteQueryBuilder From<TEntity>() where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        _tableName = metadata.TableName;
        return this;
    }

    /// <summary>
    /// Adds a WHERE condition.
    /// </summary>
    public DeleteQueryBuilder Where(string condition)
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
    public DeleteQueryBuilder Where(string column, object? value)
    {
        var paramName = AddParameter(value);
        return Where($"[{column}] = {paramName}");
    }

    /// <summary>
    /// Sets the WHERE clause for the primary key.
    /// </summary>
    public DeleteQueryBuilder WherePrimaryKey<TEntity>(TEntity entity) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        if (metadata.PrimaryKey != null)
        {
            Where(metadata.PrimaryKey.ColumnName, metadata.PrimaryKey.GetValue(entity));
        }
        return this;
    }

    /// <summary>
    /// Sets the WHERE clause for the primary key by ID.
    /// </summary>
    public DeleteQueryBuilder WherePrimaryKey<TEntity, TKey>(TKey id) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        if (metadata.PrimaryKey != null)
        {
            Where(metadata.PrimaryKey.ColumnName, id);
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
    /// Builds the DELETE query.
    /// </summary>
    public string Build()
    {
        if (_whereClause.Length == 0)
        {
            throw new InvalidOperationException("DELETE query must have a WHERE clause for safety.");
        }

        var sql = new StringBuilder();
        sql.Append($"DELETE FROM [{_tableName}]");
        sql.Append(" WHERE ");
        sql.Append(_whereClause);

        return sql.ToString();
    }
}
