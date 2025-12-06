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

    /// <summary>
    /// Gets an entity by its primary key asynchronously.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate asynchronously.
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching a predicate asynchronously, or null.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate asynchronously.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the predicate asynchronously.
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Command Operations

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    TEntity Add(TEntity entity);

    /// <summary>
    /// Adds a new entity asynchronously.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    void Delete(TEntity entity);

    /// <summary>
    /// Deletes an entity asynchronously.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    void Delete(TKey id);

    /// <summary>
    /// Deletes an entity by its primary key asynchronously.
    /// </summary>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Repository interface with int as default key type.
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class
{
}
