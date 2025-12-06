using MiniORM.Core;
using MiniORM.Core.Connection;
using MiniORM.Core.Repository;
using MiniORM.WebApi.DTOs;
using MiniORM.WebApi.Entities;

var builder = WebApplication.CreateBuilder(args);

// Register MiniORM services
// Using in-memory SQLite for demo purposes
var connectionFactory = SqliteConnectionFactory.CreateInMemory();
var dbContext = new DbContext(connectionFactory);

builder.Services.AddSingleton(dbContext);
builder.Services.AddScoped<Repository<Product>>(_ => new Repository<Product>(dbContext));

var app = builder.Build();

// Initialize database - create Products table
InitializeDatabase(dbContext);

// Map API endpoints
app.MapGet("/", () => "MiniORM WebAPI - Navigate to /api/products");

app.MapGet("/api/products", (Repository<Product> repo) =>
{
  var products = repo.GetAll()
      .Select(p => new ProductResponse(
          p.Id, p.Name, p.Description, p.Price,
          p.StockQuantity, p.CreatedAt, p.IsAvailable))
      .ToList();

  return Results.Ok(products);
});

app.MapGet("/api/products/{id:int}", (int id, Repository<Product> repo) =>
{
  var product = repo.GetById(id);

  if (product is null)
    return Results.NotFound(new { message = $"Product with ID {id} not found" });

  return Results.Ok(new ProductResponse(
      product.Id, product.Name, product.Description, product.Price,
      product.StockQuantity, product.CreatedAt, product.IsAvailable));
});

app.MapPost("/api/products", (CreateProductRequest request, Repository<Product> repo) =>
{
  var product = new Product
  {
    Name = request.Name,
    Description = request.Description,
    Price = request.Price,
    StockQuantity = request.StockQuantity,
    CreatedAt = DateTime.UtcNow,
    IsAvailable = request.IsAvailable
  };

  repo.Add(product);

  var response = new ProductResponse(
      product.Id, product.Name, product.Description, product.Price,
      product.StockQuantity, product.CreatedAt, product.IsAvailable);

  return Results.Created($"/api/products/{product.Id}", response);
});

app.MapPut("/api/products/{id:int}", (int id, UpdateProductRequest request, Repository<Product> repo) =>
{
  var product = repo.GetById(id);

  if (product is null)
    return Results.NotFound(new { message = $"Product with ID {id} not found" });

  product.Name = request.Name;
  product.Description = request.Description;
  product.Price = request.Price;
  product.StockQuantity = request.StockQuantity;
  product.IsAvailable = request.IsAvailable;

  repo.Update(product);

  return Results.Ok(new ProductResponse(
      product.Id, product.Name, product.Description, product.Price,
      product.StockQuantity, product.CreatedAt, product.IsAvailable));
});

app.MapDelete("/api/products/{id:int}", (int id, Repository<Product> repo) =>
{
  var product = repo.GetById(id);

  if (product is null)
    return Results.NotFound(new { message = $"Product with ID {id} not found" });

  repo.Delete(product);

  return Results.NoContent();
});

app.Run();

/// <summary>
/// Initialize the database with required tables.
/// </summary>
void InitializeDatabase(DbContext context)
{
  context.ExecuteNonQuery(@"
        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProductName TEXT NOT NULL,
            Description TEXT,
            Price REAL NOT NULL,
            StockQuantity INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            IsAvailable INTEGER NOT NULL
        )");

  Console.WriteLine("✓ Database initialized - Products table created");
}
