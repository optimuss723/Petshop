namespace User.Service.Entities;

/// <summary>
/// User entity trong User.Service.
/// Dùng chung schema với Auth.Service nhưng truy cập qua DbContext riêng của service này.
/// </summary>
public class User
{
    public int      Id        { get; set; }
    public string   FullName  { get; set; } = null!;
    public string   Email     { get; set; } = null!;
    public string   Phone     { get; set; } = null!;
    public string?  Address   { get; set; }
    public string   Role      { get; set; } = "Customer";
    public bool     IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
