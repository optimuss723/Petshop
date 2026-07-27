namespace Auth.Service.Entities;

/// <summary>
/// Entity User dùng trong Auth.Service.
/// Auth Service chỉ cần các trường liên quan đến xác thực.
/// </summary>
public class User
{
    public int      Id                  { get; set; }
    public string   FullName            { get; set; } = null!;
    public string   Email               { get; set; } = null!;
    public string   PasswordHash        { get; set; } = null!;
    public string   Phone               { get; set; } = null!;
    public string?  Address             { get; set; }
    public string   Role                { get; set; } = "Customer"; // Admin | Staff | Customer
    public string?  RefreshToken        { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt           { get; set; } = DateTime.UtcNow;
    public bool     IsActive            { get; set; } = true;
}
