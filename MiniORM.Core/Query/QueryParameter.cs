namespace MiniORM.Core.Query;

/// <summary>
/// Represents a query parameter.
/// </summary>
public class QueryParameter
{
    public string Name { get; }
    public object? Value { get; }
    public Type? Type { get; }

    public QueryParameter(string name, object? value, Type? type = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
        Type = type ?? value?.GetType();
    }
}
