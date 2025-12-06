using System.ComponentModel;

namespace MiniORM.Core.UnitOfWork;

/// <summary>
/// Tracks entity changes for the Unit of Work pattern.
/// 
/// Design Pattern: Observer Pattern
/// - Subscribes to INotifyPropertyChanged events
/// - Automatically tracks entity modifications
/// </summary>
public class ChangeTracker
{
    private readonly Dictionary<object, EntityEntry> _trackedEntities 
        = new Dictionary<object, EntityEntry>();

    /// <summary>
    /// Gets all tracked entity entries.
    /// </summary>
    public IReadOnlyCollection<EntityEntry> Entries => _trackedEntities.Values;

    /// <summary>
    /// Tracks an entity with the specified state.
    /// </summary>
    public EntityEntry Track<TEntity>(TEntity entity, EntityState state) where TEntity : class
    {
        if (_trackedEntities.TryGetValue(entity, out var existingEntry))
        {
            existingEntry.State = state;
            return existingEntry;
        }

        var entry = new EntityEntry(entity, state);
        _trackedEntities[entity] = entry;

        // Subscribe to property changes if entity supports it
        if (entity is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnEntityPropertyChanged;
        }

        // Also set state on EntityBase if applicable
        if (entity is EntityBase entityBase)
        {
            entityBase.State = state;
        }

        return entry;
    }

    /// <summary>
    /// Gets the entry for an entity.
    /// </summary>
    public EntityEntry? GetEntry(object entity)
    {
        return _trackedEntities.TryGetValue(entity, out var entry) ? entry : null;
    }

    /// <summary>
    /// Checks if an entity is being tracked.
    /// </summary>
    public bool IsTracking(object entity)
    {
        return _trackedEntities.ContainsKey(entity);
    }

    /// <summary>
    /// Stops tracking an entity.
    /// </summary>
    public void Untrack(object entity)
    {
        if (_trackedEntities.TryGetValue(entity, out var entry))
        {
            if (entity is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OnEntityPropertyChanged;
            }
            _trackedEntities.Remove(entity);
        }
    }

    /// <summary>
    /// Gets all entries that have changes.
    /// </summary>
    public IEnumerable<EntityEntry> GetChangedEntries()
    {
        return _trackedEntities.Values.Where(e =>
            e.State == EntityState.Added ||
            e.State == EntityState.Modified ||
            e.State == EntityState.Deleted);
    }

    /// <summary>
    /// Gets entries by state.
    /// </summary>
    public IEnumerable<EntityEntry> GetEntriesByState(EntityState state)
    {
        return _trackedEntities.Values.Where(e => e.State == state);
    }

    /// <summary>
    /// Detects changes in all tracked entities.
    /// </summary>
    public void DetectChanges()
    {
        foreach (var entry in _trackedEntities.Values)
        {
            if (entry.State == EntityState.Unchanged && entry.HasChanges())
            {
                entry.State = EntityState.Modified;
            }
        }
    }

    /// <summary>
    /// Accepts all changes after successful save.
    /// </summary>
    public void AcceptAllChanges()
    {
        var deletedEntries = _trackedEntities.Values
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        // Remove deleted entries from tracking
        foreach (var entry in deletedEntries)
        {
            Untrack(entry.Entity);
        }

        // Accept changes for remaining entries
        foreach (var entry in _trackedEntities.Values)
        {
            entry.AcceptChanges();
            
            if (entry.Entity is EntityBase entityBase)
            {
                entityBase.State = EntityState.Unchanged;
            }
        }
    }

    /// <summary>
    /// Clears all tracked entities.
    /// </summary>
    public void Clear()
    {
        foreach (var entity in _trackedEntities.Keys.ToList())
        {
            Untrack(entity);
        }
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender != null && _trackedEntities.TryGetValue(sender, out var entry))
        {
            if (entry.State == EntityState.Unchanged)
            {
                entry.State = EntityState.Modified;
            }
        }
    }
}
