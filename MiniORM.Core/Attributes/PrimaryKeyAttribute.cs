namespace MiniORM.Core.Attributes;

/// <summary>
/// Marks a property as the primary key.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class PrimaryKeyAttribute : Attribute
{
    /// <summary>
    /// Whether the primary key is auto-generated (IDENTITY/AUTOINCREMENT).
    /// </summary>
    public bool AutoGenerate { get; set; } = true;

    /// <summary>
    /// The name of the sequence (for databases that use sequences).
    /// </summary>
    public string? SequenceName { get; set; }
}
