# MiniORM Implementation Reference

## Project Structure

```
d:\Personal\c-sharp-learning\
├── MiniORM.sln
├── MiniORM.Core/              (ORM Library)
├── MiniORM.Demo/              (Console Demo)
└── MiniORM.Tests/             (Unit Tests)
```

---

## Phase 1: Project Setup ✅

- [x] Create `MiniORM.sln` solution
- [x] Create `MiniORM.Core` class library
- [x] Create `MiniORM.Demo` console app
- [x] Create `MiniORM.Tests` xUnit project
- [x] Add `Microsoft.Data.Sqlite` NuGet package

---

## Phase 2: Attributes (Metadata Pattern) ✅

| File | Purpose |
|------|---------|
| `Attributes/TableAttribute.cs` | Maps class to table name |
| `Attributes/ColumnAttribute.cs` | Maps property to column |
| `Attributes/PrimaryKeyAttribute.cs` | Marks primary key |
| `Attributes/ForeignKeyAttribute.cs` | Defines relationships |
| `Attributes/NotMappedAttribute.cs` | Excludes from mapping |

---

## Phase 3: Entity Core (Flyweight + Observer) ✅

| File | Purpose |
|------|---------|
| `Core/EntityState.cs` | Enum: Detached, Unchanged, Added, Modified, Deleted |
| `Core/EntityBase.cs` | Base class with INotifyPropertyChanged |
| `Core/PropertyMetadata.cs` | Caches property reflection info |
| `Core/EntityMetadata.cs` | Caches entity type metadata |
| `Core/EntityMapper.cs` | Maps IDataReader to entities |

---

## Phase 4: Connection Management (Singleton + Factory) ✅

| File | Purpose |
|------|---------|
| `Connection/IDbConnectionFactory.cs` | Factory interface |
| `Connection/SqliteConnectionFactory.cs` | SQLite implementation |
| `Connection/DbContext.cs` | Connection & transaction manager |

---

## Phase 5: Query Builders (Builder Pattern) ✅

| File | Purpose |
|------|---------|
| `Query/QueryParameter.cs` | Holds parameter name/value |
| `Query/SqlQueryBuilder.cs` | Builds SELECT queries |
| `Query/InsertQueryBuilder.cs` | Builds INSERT queries |
| `Query/UpdateQueryBuilder.cs` | Builds UPDATE queries |
| `Query/DeleteQueryBuilder.cs` | Builds DELETE queries |
| `Query/ExpressionParser.cs` | LINQ to SQL conversion |

---

## Phase 6: Repository Pattern ✅

| File | Purpose |
|------|---------|
| `Repository/IRepository.cs` | Generic CRUD interface |
| `Repository/Repository.cs` | Implementation with DbContext |

---

## Phase 7: Unit of Work Pattern ✅

| File | Purpose |
|------|---------|
| `UnitOfWork/EntityEntry.cs` | Tracks entity state & values |
| `UnitOfWork/ChangeTracker.cs` | Detects entity changes |
| `UnitOfWork/IUnitOfWork.cs` | Interface for UoW |
| `UnitOfWork/UnitOfWork.cs` | Coordinates saves |
| `UnitOfWork/TrackedRepository.cs` | Auto-tracking repository |

---

## Phase 8: Demo Application ✅

| File | Purpose |
|------|---------|
| `Demo/Entities/Customer.cs` | Sample entity |
| `Demo/Entities/Order.cs` | Entity with FK |
| `Demo/Program.cs` | Demo all features |

---

## Phase 9: Unit Tests ✅

| File | Tests | Coverage |
|------|-------|----------|
| `EntityMetadataTests.cs` | 11 | Metadata, caching |
| `SqlQueryBuilderTests.cs` | 17 | SELECT queries |
| `InsertUpdateDeleteQueryBuilderTests.cs` | 16 | CUD queries |
| `ExpressionParserTests.cs` | 19 | LINQ parsing |
| `ChangeTrackerTests.cs` | 22 | Change tracking |
| `RepositoryTests.cs` | 18 | CRUD operations |
| `IntegrationTests.cs` | 16 | End-to-end |

**Total: 119 tests passing**

---

## Commands

```bash
# Build
dotnet build MiniORM.sln

# Run tests
dotnet test MiniORM.Tests

# Run demo
dotnet run --project MiniORM.Demo
```

---

## Design Patterns Used

| Pattern | Component |
|---------|-----------|
| Metadata | Attributes |
| Builder | Query Builders |
| Repository | Repository classes |
| Unit of Work | UnitOfWork, ChangeTracker |
| Singleton | DbContext |
| Factory | ConnectionFactory |
| Visitor | ExpressionParser |
| Observer | EntityBase |
| Flyweight | EntityMapper cache |
