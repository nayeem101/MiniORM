using System.Linq.Expressions;
using MiniORM.Core.Query;
using MiniORM.Core.Repository;

namespace MiniORM.Core.UnitOfWork;

/// <summary>
/// A repository that automatically tracks entities.
/// Used by UnitOfWork to enable change tracking.
/// </summary>
public class TrackedRepository<TEntity> : Repository<TEntity>, IRepository<TEntity>
    where TEntity : class, new()
{
    private readonly ChangeTracker _changeTracker;

    public TrackedRepository(DbContext context, ChangeTracker changeTracker) 
        : base(context)
    {
        _changeTracker = changeTracker;
    }

    public override TEntity? GetById(int id)
    {
        var entity = base.GetById(id);
        if (entity != null)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entity;
    }

    public override IEnumerable<TEntity> GetAll()
    {
        var entities = base.GetAll().ToList();
        foreach (var entity in entities)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entities;
    }

    public override IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = base.Find(predicate).ToList();
        foreach (var entity in entities)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entities;
    }

    public override TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        var entity = base.FirstOrDefault(predicate);
        if (entity != null)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entity;
    }

    public override TEntity Add(TEntity entity)
    {
        _changeTracker.Track(entity, EntityState.Added);
        return entity; // Don't insert yet, SaveChanges will do it
    }

    public override void Update(TEntity entity)
    {
        if (!_changeTracker.IsTracking(entity))
        {
            _changeTracker.Track(entity, EntityState.Modified);
        }
        else
        {
            var entry = _changeTracker.GetEntry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Modified;
            }
        }
    }

    public override void Delete(TEntity entity)
    {
        if (!_changeTracker.IsTracking(entity))
        {
            _changeTracker.Track(entity, EntityState.Deleted);
        }
        else
        {
            var entry = _changeTracker.GetEntry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Deleted;
            }
        }
    }

    public override void Delete(int id)
    {
        var entity = GetById(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    public override async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await base.GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entity;
    }

    public override async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = (await base.GetAllAsync(cancellationToken)).ToList();
        foreach (var entity in entities)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entities;
    }

    public override async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = (await base.FindAsync(predicate, cancellationToken)).ToList();
        foreach (var entity in entities)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entities;
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entity = await base.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity != null)
        {
            _changeTracker.Track(entity, EntityState.Unchanged);
        }
        return entity;
    }

    public override Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _changeTracker.Track(entity, EntityState.Added);
        return Task.FromResult(entity); // Don't insert yet, SaveChangesAsync will do it
    }

    public override Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_changeTracker.IsTracking(entity))
        {
            _changeTracker.Track(entity, EntityState.Modified);
        }
        else
        {
            var entry = _changeTracker.GetEntry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Modified;
            }
        }
        return Task.CompletedTask;
    }

    public override Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_changeTracker.IsTracking(entity))
        {
            _changeTracker.Track(entity, EntityState.Deleted);
        }
        else
        {
            var entry = _changeTracker.GetEntry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Deleted;
            }
        }
        return Task.CompletedTask;
    }

    public override async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }
}
