namespace MiniORM.Core;

/// <summary>
/// Represents the state of an entity in the context.
/// </summary>
public enum EntityState
{
    /// <summary>
    /// Entity is not being tracked.
    /// </summary>
    Detached,

    /// <summary>
    /// Entity exists in the database and has not been modified.
    /// </summary>
    Unchanged,

    /// <summary>
    /// Entity is new and will be inserted.
    /// </summary>
    Added,

    /// <summary>
    /// Entity has been modified and will be updated.
    /// </summary>
    Modified,

    /// <summary>
    /// Entity will be deleted.
    /// </summary>
    Deleted
}
