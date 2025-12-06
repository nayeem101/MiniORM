namespace MiniORM.Core.Attributes;

/// <summary>
/// Maps a property to a database column.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// The name of the database column.
    /// If not specified, the property name is used.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The SQL data type (e.g., "NVARCHAR(100)", "INT").
    /// </summary>
    public string? DbType { get; set; }

    /// <summary>
    /// Whether the column allows NULL values.
    /// </summary>
    public bool IsNullable { get; set; } = true;

    /// <summary>
    /// Maximum length for string columns.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    public ColumnAttribute() { }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}
