using Microsoft.EntityFrameworkCore;
using Order.Service.Entities;
using OrderEntity = Order.Service.Entities.Order;
using OrderItemEntity = Order.Service.Entities.OrderItem;

namespace Order.Service.Data;

/// <summary>
/// DbContext cho Order.Service.
/// Chỉ quản lý Orders và OrderItems.
/// ProductId/PetId là loose references (không FK constraint sang bảng khác service).
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<OrderEntity>     Orders        { get; set; }
    public DbSet<OrderItemEntity> OrderItems    { get; set; }
    public DbSet<StaffAuditLog>   StaffAuditLogs{ get; set; }

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<OrderEntity>(e =>
        {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).HasMaxLength(50);
            e.Property(o => o.ShippingAddress).HasMaxLength(500);
        });

        m.Entity<OrderItemEntity>(e =>
        {
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            // Không có FK sang Product/Pet — chúng thuộc Catalog.Service
        });

        m.Entity<StaffAuditLog>(e =>
        {
            e.Property(l => l.Action).HasMaxLength(100);
            e.Property(l => l.StaffName).HasMaxLength(200);
            e.HasIndex(l => l.StaffId);
            e.HasIndex(l => l.CreatedAt);
        });
    }
}
