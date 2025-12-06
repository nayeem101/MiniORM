using System.Linq.Expressions;

namespace MiniORM.Core.Repository;

/// <summary>
/// Generic repository interface for data access.
/// 
/// Design Pattern: Repository Pattern
/// - Centralizes data access logic
/// - Provides clean API for CRUD operations
/// - Makes code more testable
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    #region Query Operations

    /// <summary>
    /// Gets an entity by its primary key.
    /// </summary>
    TEntity? GetById(TKey id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    IEnumerable<TEntity> GetAll();

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets the first entity matching a predicate, or null.
    /// </summary>
    TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    bool Any(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    int Count(Expression<Func<TEntity, bool>>? predicate = null);

    #endregion

    #region Command Operations

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    TEntity Add(TEntity entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    void Delete(TEntity entity);

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    void Delete(TKey id);

    #endregion
}

/// <summary>
/// Repository interface with int as default key type.
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class
{
}
