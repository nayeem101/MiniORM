using MiniORM.Core;
using MiniORM.Core.Attributes;

namespace MiniORM.Tests;

/// <summary>
/// Test entity for use in unit tests.
/// </summary>
[Table("TestCustomers")]
public class TestCustomer : EntityBase
{
    private int _id;
    private string _name = "";
    private string _email = "";
    private int _age;
    private decimal _balance;
    private DateTime _createdAt;
    private bool _isActive;

    [PrimaryKey(AutoGenerate = true)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [Column("CustomerName", MaxLength = 100)]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [Column(MaxLength = 255)]
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    [NotMapped]
    public string DisplayName => $"{Name} <{Email}>";
}

/// <summary>
/// Test entity with foreign key relationship.
/// </summary>
[Table("TestOrders")]
public class TestOrder : EntityBase
{
    private int _id;
    private int _customerId;
    private string _product = "";
    private decimal _total;

    [PrimaryKey(AutoGenerate = true)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [ForeignKey(typeof(TestCustomer))]
    public int CustomerId
    {
        get => _customerId;
        set => SetProperty(ref _customerId, value);
    }

    public string Product
    {
        get => _product;
        set => SetProperty(ref _product, value);
    }

    public decimal Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }
}
