using MiniORM.Core;
using MiniORM.Core.UnitOfWork;

namespace MiniORM.Tests;

/// <summary>
/// Tests for ChangeTracker and EntityEntry classes.
/// </summary>
public class ChangeTrackerTests
{
    #region Track Tests

    [Fact]
    public void Track_NewEntity_AddsToTracker()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };

        // Act
        var entry = tracker.Track(customer, EntityState.Added);

        // Assert
        Assert.NotNull(entry);
        Assert.Same(customer, entry.Entity);
        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public void Track_SameEntityTwice_ReturnsSameEntry()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };

        // Act
        var entry1 = tracker.Track(customer, EntityState.Unchanged);
        var entry2 = tracker.Track(customer, EntityState.Modified);

        // Assert
        Assert.Same(entry1, entry2);
        Assert.Equal(EntityState.Modified, entry1.State);  // State updated
    }

    [Fact]
    public void Track_SetsEntityBaseState()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };

        // Act
        tracker.Track(customer, EntityState.Unchanged);

        // Assert
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    #endregion

    #region GetEntry Tests

    [Fact]
    public void GetEntry_TrackedEntity_ReturnsEntry()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };
        tracker.Track(customer, EntityState.Unchanged);

        // Act
        var entry = tracker.GetEntry(customer);

        // Assert
        Assert.NotNull(entry);
        Assert.Same(customer, entry.Entity);
    }

    [Fact]
    public void GetEntry_UntrackedEntity_ReturnsNull()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };

        // Act
        var entry = tracker.GetEntry(customer);

        // Assert
        Assert.Null(entry);
    }

    #endregion

    #region IsTracking Tests

    [Fact]
    public void IsTracking_TrackedEntity_ReturnsTrue()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };
        tracker.Track(customer, EntityState.Unchanged);

        // Act & Assert
        Assert.True(tracker.IsTracking(customer));
    }

    [Fact]
    public void IsTracking_UntrackedEntity_ReturnsFalse()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };

        // Act & Assert
        Assert.False(tracker.IsTracking(customer));
    }

    #endregion

    #region GetChangedEntries Tests

    [Fact]
    public void GetChangedEntries_ReturnsOnlyChangedEntities()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var added = new TestCustomer { Name = "Added" };
        var modified = new TestCustomer { Name = "Modified" };
        var deleted = new TestCustomer { Name = "Deleted" };
        var unchanged = new TestCustomer { Name = "Unchanged" };

        tracker.Track(added, EntityState.Added);
        tracker.Track(modified, EntityState.Modified);
        tracker.Track(deleted, EntityState.Deleted);
        tracker.Track(unchanged, EntityState.Unchanged);

        // Act
        var changed = tracker.GetChangedEntries().ToList();

        // Assert
        Assert.Equal(3, changed.Count);
        Assert.Contains(changed, e => e.Entity == added);
        Assert.Contains(changed, e => e.Entity == modified);
        Assert.Contains(changed, e => e.Entity == deleted);
        Assert.DoesNotContain(changed, e => e.Entity == unchanged);
    }

    [Fact]
    public void GetEntriesByState_FiltersCorrectly()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var added1 = new TestCustomer { Name = "Added1" };
        var added2 = new TestCustomer { Name = "Added2" };
        var modified = new TestCustomer { Name = "Modified" };

        tracker.Track(added1, EntityState.Added);
        tracker.Track(added2, EntityState.Added);
        tracker.Track(modified, EntityState.Modified);

        // Act
        var addedEntries = tracker.GetEntriesByState(EntityState.Added).ToList();

        // Assert
        Assert.Equal(2, addedEntries.Count);
    }

    #endregion

    #region DetectChanges Tests

    [Fact]
    public void DetectChanges_ModifiedEntity_SetsStateToModified()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "Original" };
        tracker.Track(customer, EntityState.Unchanged);

        // Act - Modify the entity
        customer.Name = "Modified";
        tracker.DetectChanges();

        // Assert
        var entry = tracker.GetEntry(customer);
        Assert.Equal(EntityState.Modified, entry!.State);
    }

    [Fact]
    public void DetectChanges_UnmodifiedEntity_RemainsUnchanged()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "Original" };
        tracker.Track(customer, EntityState.Unchanged);

        // Act - Don't modify, just detect
        tracker.DetectChanges();

        // Assert
        var entry = tracker.GetEntry(customer);
        Assert.Equal(EntityState.Unchanged, entry!.State);
    }

    #endregion

    #region AcceptAllChanges Tests

    [Fact]
    public void AcceptAllChanges_ResetsAddedToUnchanged()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "New" };
        tracker.Track(customer, EntityState.Added);

        // Act
        tracker.AcceptAllChanges();

        // Assert
        var entry = tracker.GetEntry(customer);
        Assert.Equal(EntityState.Unchanged, entry!.State);
        Assert.Equal(EntityState.Unchanged, customer.State);
    }

    [Fact]
    public void AcceptAllChanges_ResetsModifiedToUnchanged()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "Original" };
        tracker.Track(customer, EntityState.Modified);

        // Act
        tracker.AcceptAllChanges();

        // Assert
        var entry = tracker.GetEntry(customer);
        Assert.Equal(EntityState.Unchanged, entry!.State);
    }

    [Fact]
    public void AcceptAllChanges_RemovesDeletedFromTracking()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "ToDelete" };
        tracker.Track(customer, EntityState.Deleted);

        // Act
        tracker.AcceptAllChanges();

        // Assert
        Assert.False(tracker.IsTracking(customer));
    }

    #endregion

    #region Untrack Tests

    [Fact]
    public void Untrack_RemovesEntityFromTracker()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var customer = new TestCustomer { Name = "John" };
        tracker.Track(customer, EntityState.Unchanged);

        // Act
        tracker.Untrack(customer);

        // Assert
        Assert.False(tracker.IsTracking(customer));
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_RemovesAllEntities()
    {
        // Arrange
        var tracker = new ChangeTracker();
        tracker.Track(new TestCustomer { Name = "A" }, EntityState.Added);
        tracker.Track(new TestCustomer { Name = "B" }, EntityState.Modified);
        tracker.Track(new TestCustomer { Name = "C" }, EntityState.Unchanged);

        // Act
        tracker.Clear();

        // Assert
        Assert.Empty(tracker.Entries);
    }

    #endregion

    #region EntityEntry Tests

    [Fact]
    public void EntityEntry_HasChanges_ReturnsTrueWhenModified()
    {
        // Arrange
        var customer = new TestCustomer { Name = "Original", Email = "test@test.com" };
        var entry = new EntityEntry(customer, EntityState.Unchanged);

        // Act
        customer.Name = "Modified";
        var hasChanges = entry.HasChanges();

        // Assert
        Assert.True(hasChanges);
    }

    [Fact]
    public void EntityEntry_HasChanges_ReturnsFalseWhenUnmodified()
    {
        // Arrange
        var customer = new TestCustomer { Name = "Original", Email = "test@test.com" };
        var entry = new EntityEntry(customer, EntityState.Unchanged);

        // Act
        var hasChanges = entry.HasChanges();

        // Assert
        Assert.False(hasChanges);
    }

    [Fact]
    public void EntityEntry_GetModifiedProperties_ReturnsChangedProperties()
    {
        // Arrange
        var customer = new TestCustomer { Name = "Original", Email = "test@test.com", Age = 25 };
        var entry = new EntityEntry(customer, EntityState.Unchanged);

        // Act
        customer.Name = "Modified";
        customer.Age = 30;
        var modified = entry.GetModifiedProperties().ToList();

        // Assert
        Assert.Contains("Name", modified);
        Assert.Contains("Age", modified);
        Assert.DoesNotContain("Email", modified);
    }

    [Fact]
    public void EntityEntry_AcceptChanges_UpdatesOriginalValues()
    {
        // Arrange
        var customer = new TestCustomer { Name = "Original" };
        var entry = new EntityEntry(customer, EntityState.Modified);
        customer.Name = "Modified";

        // Act
        entry.AcceptChanges();

        // Assert
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(entry.HasChanges());
    }

    #endregion
}
