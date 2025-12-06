using MiniORM.Core;
using MiniORM.Core.Attributes;

namespace MiniORM.Demo.Entities;

/// <summary>
/// Sample Customer entity demonstrating attribute-based mapping.
/// </summary>
[Table("Customers")]
public class Customer : EntityBase
{
    private int _id;
    private string _name = "";
    private string _email = "";
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
