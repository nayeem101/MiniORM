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
}
