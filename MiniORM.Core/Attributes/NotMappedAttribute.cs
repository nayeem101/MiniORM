namespace MiniORM.Core.Attributes;

/// <summary>
/// Excludes a property from database mapping.
/// Useful for computed properties or navigation properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class NotMappedAttribute : Attribute
{
}
