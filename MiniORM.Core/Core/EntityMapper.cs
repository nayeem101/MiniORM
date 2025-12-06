using System.Collections.Concurrent;
using System.Data;

namespace MiniORM.Core;

/// <summary>
/// Maps between database records and entity objects.
/// 
/// Design Pattern: Flyweight Pattern
/// - Caches EntityMetadata instances to avoid repeated reflection
/// 
/// Design Pattern: Object Pool (via caching)
/// - Reuses metadata objects across multiple operations
/// </summary>
public static class EntityMapper
{
    // Thread-safe cache for entity metadata
    private static readonly ConcurrentDictionary<Type, EntityMetadata> _metadataCache
        = new ConcurrentDictionary<Type, EntityMetadata>();

    /// <summary>
    /// Gets or creates metadata for an entity type.
    /// Uses caching to avoid repeated reflection overhead.
    /// </summary>
    public static EntityMetadata GetMetadata<TEntity>() where TEntity : class
    {
        return GetMetadata(typeof(TEntity));
    }

    /// <summary>
    /// Gets or creates metadata for an entity type.
    /// </summary>
    public static EntityMetadata GetMetadata(Type entityType)
    {
        // GetOrAdd is atomic - ensures we only create metadata once per type
        return _metadataCache.GetOrAdd(entityType, type => new EntityMetadata(type));
    }

    /// <summary>
    /// Maps a DataReader row to an entity instance.
    /// </summary>
    public static TEntity MapFromReader<TEntity>(IDataReader reader) where TEntity : class, new()
    {
        var metadata = GetMetadata<TEntity>();
        var entity = new TEntity();

        // Get column names from the reader for efficient lookup
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columnNames.Add(reader.GetName(i));
        }

        // Map each property
        foreach (var property in metadata.MappedProperties)
        {
            if (columnNames.Contains(property.ColumnName))
            {
                var ordinal = reader.GetOrdinal(property.ColumnName);
                var value = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                property.SetValue(entity, value);
            }
        }

        // Set entity state to Unchanged (it came from the database)
        if (entity is EntityBase entityBase)
        {
            entityBase.State = EntityState.Unchanged;
        }

        return entity;
    }

    /// <summary>
    /// Maps multiple DataReader rows to a list of entities.
    /// </summary>
    public static List<TEntity> MapAllFromReader<TEntity>(IDataReader reader) where TEntity : class, new()
    {
        var results = new List<TEntity>();

        while (reader.Read())
        {
            results.Add(MapFromReader<TEntity>(reader));
        }

        return results;
    }

    /// <summary>
    /// Converts an entity to a dictionary of column names and values.
    /// Useful for generating SQL parameters.
    /// </summary>
    public static Dictionary<string, object?> ToDictionary<TEntity>(TEntity entity) where TEntity : class
    {
        var metadata = GetMetadata<TEntity>();
        var dict = new Dictionary<string, object?>();

        foreach (var property in metadata.MappedProperties)
        {
            dict[property.ColumnName] = property.GetValue(entity) ?? DBNull.Value;
        }

        return dict;
    }

    /// <summary>
    /// Clears the metadata cache. Useful for testing or dynamic schema changes.
    /// </summary>
    public static void ClearCache()
    {
        _metadataCache.Clear();
    }
}
