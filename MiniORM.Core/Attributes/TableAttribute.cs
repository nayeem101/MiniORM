namespace MiniORM.Core.Attributes;

/// <summary>
/// Maps a class to a database table.
/// Design Pattern: Metadata Pattern
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class TableAttribute : Attribute
{
    /// <summary>
    /// The name of the database table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional schema name (e.g., "dbo" in SQL Server).
    /// </summary>
    public string Schema { get; set; } = "";

    public TableAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the fully qualified table name.
    /// </summary>
    public string FullTableName => string.IsNullOrEmpty(Schema)
        ? $"[{Name}]"
        : $"[{Schema}].[{Name}]";
}
