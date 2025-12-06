namespace MiniORM.WebApi.DTOs;

/// <summary>
/// DTO for creating a new product.
/// </summary>
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsAvailable = true
);

/// <summary>
/// DTO for updating an existing product.
/// </summary>
public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsAvailable
);

/// <summary>
/// DTO for returning product data.
/// </summary>
public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    bool IsAvailable
);
