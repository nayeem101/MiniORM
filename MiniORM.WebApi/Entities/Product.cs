using MiniORM.Core;
using MiniORM.Core.Attributes;

namespace MiniORM.WebApi.Entities;

/// <summary>
/// Product entity for the WebAPI demo.
/// </summary>
[Table("Products")]
public class Product : EntityBase
{
    private int _id;
    private string _name = "";
    private string _description = "";
    private decimal _price;
    private int _stockQuantity;
    private DateTime _createdAt;
    private bool _isAvailable;

    [PrimaryKey(AutoGenerate = true)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [Column("ProductName", MaxLength = 100)]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [Column(MaxLength = 500)]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public int StockQuantity
    {
        get => _stockQuantity;
        set => SetProperty(ref _stockQuantity, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }
}
