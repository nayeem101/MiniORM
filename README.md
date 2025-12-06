# C# ORM Implementation Guide

This document is about implementing a simple ORM for SQL databases in C#.

---

## Table of Contents

1. [Overview & Architecture](#overview--architecture)
2. [Project Setup](#project-setup)
3. [Custom Attributes (Metadata Pattern)](#custom-attributes)
4. [Entity Base Class](#entity-base-class)
5. [Reflection-Based Mapper](#reflection-based-mapper)
6. [Database Connection Manager (Singleton Pattern)](#database-connection-manager)
7. [SQL Query Builder (Builder Pattern)](#sql-query-builder)
8. [Generic Repository (Repository Pattern)](#generic-repository)
9. [Unit of Work Pattern](#unit-of-work-pattern)
10. [Expression Parser for LINQ-like Queries](#expression-parser)
11. [Putting It All Together](#complete-example)

---

## Key Design Patterns Used

### 1. **Metadata Pattern (Attributes)**
- `[Table]`, `[Column]`, `[PrimaryKey]`, `[ForeignKey]` attributes
- Declarative way to define database mappings
- Keeps mapping information close to entity definitions

### 2. **Builder Pattern (SQL Query Builder)**
- Fluent interface for constructing SQL queries
- Separates construction from representation
- Method chaining for readable code

### 3. **Repository Pattern**
- Mediates between domain and data mapping layers
- Centralizes data access logic
- Makes code more testable through mocking

### 4. **Unit of Work Pattern**
- Maintains list of objects affected by a transaction
- Coordinates writing out changes
- Ensures consistency across repositories

### 5. **Singleton Pattern (Connection Factory)**
- Ensures single instance of connection factory
- Thread-safe implementation with double-check locking
- Manages database connections centrally

### 6. **Visitor Pattern (Expression Parser)**
- Visits nodes in expression trees
- Generates SQL from LINQ expressions
- Enables type-safe query construction

### 7. **Observer Pattern (Change Tracking)**
- Uses `INotifyPropertyChanged` interface
- Tracks entity state changes
- Automatic detection of modifications

### 8. **Factory Pattern (Connection Factory)**
- Encapsulates object creation logic
- Supports multiple database providers
- Abstract Factory for different connection types

### 9. **Flyweight Pattern (Metadata Caching)**
- Caches `EntityMetadata` instances
- Avoids repeated reflection overhead
- Reuses metadata objects

### 10. **Template Method Pattern (EntityBase)**
- Provides skeleton for entity behavior
- Subclasses implement specific properties
- Common functionality in base class

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐  │
│  │   Entity    │    │  Repository │    │    Unit of Work     │  │
│  │   Classes   │    │   Pattern   │    │      Pattern        │  │
│  └──────┬──────┘    └──────┬──────┘    └──────────┬──────────┘  │
│         │                  │                       │             │
│         ▼                  ▼                       ▼             │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                      ORM Core                                ││
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   ││
│  │  │  Attributes  │  │   Mapper     │  │  Query Builder   │   ││
│  │  │  (Metadata)  │  │ (Reflection) │  │    (Builder)     │   ││
│  │  └──────────────┘  └──────────────┘  └──────────────────┘   ││
│  └─────────────────────────────────────────────────────────────┘│
│                              │                                   │
│                              ▼                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │              Database Connection Manager                     ││
│  │                    (Singleton Pattern)                       ││
│  └─────────────────────────────────────────────────────────────┘│
│                              │                                   │
└──────────────────────────────┼───────────────────────────────────┘
                               ▼
                    ┌─────────────────────┐
                    │   SQL Database      │
                    │ (SQL Server/SQLite) │
                    └─────────────────────┘
```

---

## Project Structure

```
MiniORM/
├── MiniORM.Core/
│   ├── Attributes/
│   │   ├── TableAttribute.cs
│   │   ├── ColumnAttribute.cs
│   │   ├── PrimaryKeyAttribute.cs
│   │   ├── ForeignKeyAttribute.cs
│   │   └── NotMappedAttribute.cs
│   ├── Core/
│   │   ├── EntityBase.cs
│   │   ├── EntityMapper.cs
│   │   └── EntityMetadata.cs
│   ├── Connection/
│   │   ├── IDbConnectionFactory.cs
│   │   ├── DbConnectionFactory.cs
│   │   └── DbContext.cs
│   ├── Query/
│   │   ├── SqlQueryBuilder.cs
│   │   ├── InsertQueryBuilder.cs
│   │   ├── UpdateQueryBuilder.cs
│   │   ├── DeleteQueryBuilder.cs
│   │   └── ExpressionParser.cs
│   ├── Repository/
│   │   ├── IRepository.cs
│   │   └── Repository.cs
│   └── UnitOfWork/
│       ├── IUnitOfWork.cs
│       ├── UnitOfWork.cs
│       └── ChangeTracker.cs
└── MiniORM.Demo/
    ├── Program.cs
    └── Entities/
        ├── Customer.cs
        ├── Order.cs
        └── Product.cs
```

---

## Key Components Explained

### 1. Custom Attributes (Metadata)

Attributes allow you to attach metadata to your entity classes declaratively:

- `[Table("TableName")]` - Maps class to database table
- `[Column("ColumnName")]` - Maps property to database column
- `[PrimaryKey]` - Marks property as primary key
- `[ForeignKey(typeof(RelatedEntity))]` - Defines relationships
- `[NotMapped]` - Excludes property from mapping

### 2. Entity Base Class

Provides common functionality:
- Change tracking via `INotifyPropertyChanged`
- Entity state management (Detached, Unchanged, Added, Modified, Deleted)
- Helper methods for property setters

### 3. Entity Mapper

Uses reflection to:
- Read attribute metadata at runtime
- Cache metadata for performance (Flyweight pattern)
- Map database rows to objects
- Convert objects to dictionaries for SQL parameters

### 4. Query Builders

Fluent API for constructing SQL:
- `SqlQueryBuilder` - SELECT queries with WHERE, JOIN, ORDER BY, etc.
- `InsertQueryBuilder` - INSERT statements
- `UpdateQueryBuilder` - UPDATE statements
- `DeleteQueryBuilder` - DELETE statements

### 5. Repository

Generic CRUD operations:
- `GetById(id)` - Retrieve by primary key
- `GetAll()` - Retrieve all entities
- `Find(predicate)` - Query with LINQ expressions
- `Add(entity)` - Insert new entity
- `Update(entity)` - Update existing entity
- `Delete(entity)` - Remove entity

### 6. Unit of Work

Coordinates changes:
- Tracks entity state changes
- Groups operations into transactions
- Calls `SaveChanges()` to persist all changes
- Supports rollback on errors

### 7. Expression Parser

Converts LINQ to SQL:
- Parses expression trees
- Handles operators (==, !=, <, >, &&, ||)
- Supports string methods (Contains, StartsWith, EndsWith)
- Prevents SQL injection with parameterized queries

---

## Usage Example

```csharp
// Define entities
[Table("Customers")]
public class Customer : EntityBase
{
    [PrimaryKey(AutoGenerate = true)]
    public int Id { get; set; }
    
    [Column("CustomerName")]
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    [NotMapped]
    public string DisplayName => $"{Name} ({Email})";
}

// Setup
var connectionFactory = new SqlConnectionFactory(connectionString);
var context = new DbContext(connectionFactory);
var unitOfWork = new UnitOfWork(context);

// Query
var customerRepo = unitOfWork.Repository<Customer>();
var customers = customerRepo.Find(c => c.Name.Contains("John"));

// Insert
var newCustomer = new Customer { Name = "John Doe", Email = "john@example.com" };
customerRepo.Add(newCustomer);

// Update
newCustomer.Email = "newemail@example.com";
customerRepo.Update(newCustomer);

// Save changes
unitOfWork.SaveChanges();
```

---

## Learning Outcomes

By implementing this ORM, you'll learn about:

1. **C# Advanced Features**
   - Attributes and reflection
   - Expression trees
   - Generics and constraints
   - Events and delegates
   - Extension methods
   - LINQ

2. **Design Patterns**
   - Repository pattern
   - Unit of Work pattern
   - Builder pattern
   - Factory pattern
   - Singleton pattern
   - Observer pattern
   - Visitor pattern

3. **Database Concepts**
   - ADO.NET and IDbConnection
   - Parameterized queries
   - Transactions
   - Connection management

4. **Software Engineering Principles**
   - Separation of concerns
   - Single responsibility principle
   - Dependency injection
   - Interface-based design
   - Testability

---

## Next Steps for Enhancement

1. **Add async/await support** throughout the ORM
2. **Implement lazy loading** for related entities
3. **Add caching layer** to reduce database calls
4. **Support for complex relationships** (one-to-many, many-to-many)
5. **Migration support** for database schema changes
6. **Better LINQ provider** with full IQueryable implementation
7. **Multiple database support** (PostgreSQL, MySQL, etc.)
8. **Object pooling** for connections and commands
9. **Optimistic concurrency** with row versioning
10. **Logging and diagnostics** for query execution

---

## Resources

- **Entity Framework Core** - Microsoft's official ORM (study this for real-world usage)
- **Dapper** - Micro-ORM for inspiration on simplicity
- **C# Language Specification** - For expression trees and reflection
- **Design Patterns** - Gang of Four book

---

*This guide is meant for educational purposes to understand ORM internals. For production use, consider established ORMs like Entity Framework Core or Dapper.*

