using Microsoft.EntityFrameworkCore;
using User.Service.Entities;

namespace User.Service.Data;

/// <summary>
/// DbContext cho User.Service.
/// Truy cập cùng bảng Users với Auth.Service nhưng thông qua DbContext độc lập.
/// User.Service chỉ đọc/cập nhật thông tin profile, không xử lý password/token.
/// </summary>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<Entities.User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasMaxLength(50);
            e.Property(u => u.FullName).HasMaxLength(200);
        });
    }
}
