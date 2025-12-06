using MiniORM.Core;
using MiniORM.Core.Attributes;

namespace MiniORM.Demo.Entities;

/// <summary>
/// Sample Order entity with foreign key relationship.
/// </summary>
[Table("Orders")]
public class Order : EntityBase
{
    private int _id;
    private int _customerId;
    private string _productName = "";
    private decimal _total;
    private DateTime _orderDate;

    [PrimaryKey(AutoGenerate = true)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [ForeignKey(typeof(Customer))]
    public int CustomerId
    {
        get => _customerId;
        set => SetProperty(ref _customerId, value);
    }

    [Column(MaxLength = 200)]
    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public decimal Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    public DateTime OrderDate
    {
        get => _orderDate;
        set => SetProperty(ref _orderDate, value);
    }
}
