namespace zenvy.domain.Entities;

public class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; }

    public ICollection<User> Users { get; set; } = [];
}

public class UserRoles
{
    public const string Admin = "Admin";

    public const string Manager = "Manager";

    public const string TeamLead = "TeamLead";

    public const string InventoryManager =
        "InventoryManager";

    public const string Accountant =
        "Accountant";

    public const string SalesPerson =
        "SalesPerson";
}