using System.ComponentModel;
using System.Runtime.CompilerServices;
using MiniORM.Core.Attributes;

namespace MiniORM.Core;

/// <summary>
/// Base class for all entities.
/// Implements INotifyPropertyChanged for change tracking.
/// 
/// Design Pattern: Template Method Pattern
/// - Provides a skeleton for entity behavior
/// - Subclasses fill in specific details (properties)
/// 
/// Design Pattern: Observer Pattern
/// - Raises events when properties change
/// </summary>
public abstract class EntityBase : INotifyPropertyChanged
{
    /// <summary>
    /// Event for property change notifications (used by change tracker).
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Entity state for tracking changes.
    /// </summary>
    [NotMapped]
    public EntityState State { get; internal set; } = EntityState.Detached;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// The CallerMemberName attribute automatically captures the property name.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Mark as modified when a property changes (if already attached)
        if (State == EntityState.Unchanged)
        {
            State = EntityState.Modified;
        }
    }

    /// <summary>
    /// Helper method to set property values with change notification.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">Reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name (auto-captured).</param>
    /// <returns>True if the value changed, false otherwise.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        // Check if value actually changed
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
