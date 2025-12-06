using System.Text;

namespace MiniORM.Core.Query;

/// <summary>
/// Builds SQL INSERT queries.
/// </summary>
public class InsertQueryBuilder
{
    private string _tableName = "";
    private readonly List<string> _columns = new List<string>();
    private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
    private int _parameterIndex = 0;
    private bool _returnId = true;

    /// <summary>
    /// Gets all parameters added to this query.
    /// </summary>
    public IReadOnlyList<QueryParameter> Parameters => _parameters;

    /// <summary>
    /// Sets the table to insert into.
    /// </summary>
    public InsertQueryBuilder Into(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    /// <summary>
    /// Sets the table to insert into using entity metadata.
    /// </summary>
    public InsertQueryBuilder Into<TEntity>() where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        _tableName = metadata.TableName;
        return this;
    }

    /// <summary>
    /// Adds a column and value.
    /// </summary>
    public InsertQueryBuilder Value(string column, object? value)
    {
        _columns.Add(column);
        var paramName = $"@p{_parameterIndex++}";
        _parameters.Add(new QueryParameter(paramName, value));
        return this;
    }

    /// <summary>
    /// Adds all mapped properties from an entity.
    /// </summary>
    public InsertQueryBuilder Values<TEntity>(TEntity entity) where TEntity : class
    {
        var metadata = EntityMapper.GetMetadata<TEntity>();
        foreach (var prop in metadata.GetInsertProperties())
        {
            Value(prop.ColumnName, prop.GetValue(entity));
        }
        return this;
    }

    /// <summary>
    /// Sets whether to return the inserted ID.
    /// </summary>
    public InsertQueryBuilder ReturnId(bool returnId = true)
    {
        _returnId = returnId;
        return this;
    }

    /// <summary>
    /// Gets parameters as tuples for DbContext execution.
    /// </summary>
    public (string name, object? value)[] GetParameterTuples()
    {
        return _parameters.Select(p => (p.Name, p.Value)).ToArray();
    }

    /// <summary>
    /// Builds the INSERT query.
    /// </summary>
    public string Build()
    {
        var sql = new StringBuilder();
        sql.Append($"INSERT INTO [{_tableName}] (");
        sql.Append(string.Join(", ", _columns.Select(c => $"[{c}]")));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", _parameters.Select(p => p.Name)));
        sql.Append(")");

        if (_returnId)
        {
            // SQLite uses last_insert_rowid()
            sql.Append("; SELECT last_insert_rowid()");
        }

        return sql.ToString();
    }
}
