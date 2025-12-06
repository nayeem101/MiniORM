namespace MiniORM.Core.Attributes;

/// <summary>
/// Marks a property as a foreign key relationship.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ForeignKeyAttribute : Attribute
{
    /// <summary>
    /// The type of the related entity.
    /// </summary>
    public Type RelatedEntity { get; }

    /// <summary>
    /// The property name on the related entity (usually the primary key).
    /// </summary>
    public string RelatedProperty { get; set; } = "Id";

    public ForeignKeyAttribute(Type relatedEntity)
    {
        RelatedEntity = relatedEntity ?? throw new ArgumentNullException(nameof(relatedEntity));
    }
}
