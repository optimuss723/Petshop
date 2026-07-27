using Auth.Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Service.Data;

/// <summary>
/// DbContext cho Auth.Service.
/// Chỉ truy cập bảng Users — đúng nguyên tắc Microservice (database per service).
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Seed dữ liệu mặc định
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id           = 1,
                FullName     = "Admin PetShop",
                Email        = "admin@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Phone        = "0901234567",
                Role         = "Admin",
                CreatedAt    = new DateTime(2024, 1, 1)
            },
            new User
            {
                Id           = 2,
                FullName     = "Nhân viên 1",
                Email        = "staff@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Phone        = "0901234568",
                Role         = "Staff",
                CreatedAt    = new DateTime(2024, 1, 1)
            },
            new User
            {
                Id           = 3,
                FullName     = "Nguyễn Văn A",
                Email        = "customer@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Phone        = "0901234569",
                Role         = "Customer",
                CreatedAt    = new DateTime(2024, 1, 1)
            }
        );
    }
}
