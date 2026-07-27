namespace PetShop.Shared.Constants;

public static class Roles
{
    public const string Admin    = "Admin";
    public const string Staff    = "Staff";
    public const string Customer = "Customer";
    public const string AdminOrStaff = "Admin,Staff";
}

public static class ApiRoutes
{
    public const string Base     = "api";
    public const string Auth     = $"{Base}/auth";
    public const string Products = $"{Base}/products";
    public const string Pets     = $"{Base}/pets";
    public const string Orders   = $"{Base}/orders";
    public const string Users    = $"{Base}/users";
}
