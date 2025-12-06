using MiniORM.Core.Repository;

namespace MiniORM.Core.UnitOfWork;

/// <summary>
/// Unit of Work interface.
/// 
/// Design Pattern: Unit of Work
/// - Coordinates changes across multiple repositories
/// - Manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, new();

    /// <summary>
    /// Gets the change tracker.
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    int SaveChanges();

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    void Commit();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    void Rollback();
}
