namespace MiniORM.Core.UnitOfWork;

/// <summary>
/// Tracks the state of an entity and its original values.
/// </summary>
public class EntityEntry
{
    public object Entity { get; }
    public Type EntityType { get; }
    public EntityState State { get; set; }
    public Dictionary<string, object?> OriginalValues { get; }
    public Dictionary<string, object?> CurrentValues { get; private set; }

    public EntityEntry(object entity, EntityState state)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        EntityType = entity.GetType();
        State = state;
        OriginalValues = CaptureValues(entity);
        CurrentValues = new Dictionary<string, object?>(OriginalValues);
    }

    /// <summary>
    /// Captures all property values of an entity.
    /// </summary>
    private static Dictionary<string, object?> CaptureValues(object entity)
    {
        var values = new Dictionary<string, object?>();
        var metadata = EntityMapper.GetMetadata(entity.GetType());

        foreach (var prop in metadata.MappedProperties)
        {
            values[prop.PropertyName] = prop.GetValue(entity);
        }

        return values;
    }

    /// <summary>
    /// Updates current values from the entity.
    /// </summary>
    public void RefreshCurrentValues()
    {
        CurrentValues = CaptureValues(Entity);
    }

    /// <summary>
    /// Checks if the entity has been modified.
    /// </summary>
    public bool HasChanges()
    {
        RefreshCurrentValues();
        
        foreach (var kvp in OriginalValues)
        {
            if (!CurrentValues.TryGetValue(kvp.Key, out var currentValue))
                return true;

            if (!Equals(kvp.Value, currentValue))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the modified properties.
    /// </summary>
    public IEnumerable<string> GetModifiedProperties()
    {
        RefreshCurrentValues();
        
        foreach (var kvp in OriginalValues)
        {
            if (!CurrentValues.TryGetValue(kvp.Key, out var currentValue) ||
                !Equals(kvp.Value, currentValue))
            {
                yield return kvp.Key;
            }
        }
    }

    /// <summary>
    /// Accepts changes by updating original values to current.
    /// </summary>
    public void AcceptChanges()
    {
        RefreshCurrentValues();
        OriginalValues.Clear();
        foreach (var kvp in CurrentValues)
        {
            OriginalValues[kvp.Key] = kvp.Value;
        }
        State = EntityState.Unchanged;
    }
}
