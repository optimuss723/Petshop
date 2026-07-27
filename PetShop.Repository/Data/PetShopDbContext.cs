using Microsoft.EntityFrameworkCore;
using PetShop.Repository.Entities;
using PetShop.Shared.Enums;

namespace PetShop.Repository.Data;

public class PetShopDbContext : DbContext
{
    public PetShopDbContext(DbContextOptions<PetShopDbContext> options) : base(options) { }

    public DbSet<User>      Users     { get; set; }
    public DbSet<Product>   Products  { get; set; }
    public DbSet<Pet>       Pets      { get; set; }
    public DbSet<Order>     Orders    { get; set; }
    public DbSet<OrderItem> OrderItems{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e => {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Product>(e => {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasConversion<string>();
        });

        modelBuilder.Entity<Pet>(e => {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasConversion<string>();
            e.Property(p => p.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Order>(e => {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).HasConversion<string>();
            e.HasOne(o => o.User).WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e => {
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Order).WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(oi => oi.Pet).WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.PetId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User {
                Id = 1, FullName = "Admin PetShop", Email = "admin@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Phone = "0901234567", Role = UserRole.Admin,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new User {
                Id = 2, FullName = "Nhân viên 1", Email = "staff@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Phone = "0901234568", Role = UserRole.Staff,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new User {
                Id = 3, FullName = "Nguyễn Văn A", Email = "customer@petshop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Phone = "0901234569", Role = UserRole.Customer,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );

        modelBuilder.Entity<Product>().HasData(
            // ── THỨC ĂN (Food) ────────────────────────────────────────
            new Product { Id=1,  Name="Thức ăn Royal Canin cho chó",       Description="Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein và omega-3, giúp lông bóng mượt",          Price=350000,  Stock=100, Category=ProductCategory.Food,      IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=2,  Name="Thức ăn Whiskas cho mèo",            Description="Thức ăn ướt cho mèo mọi lứa tuổi, hương cá ngừ thơm ngon, bổ sung taurine tốt cho mắt",          Price=45000,   Stock=200, Category=ProductCategory.Food,      IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=3,  Name="Thức ăn Pedigree cho chó con",       Description="Thức ăn chuyên dụng cho chó con dưới 1 tuổi, giàu canxi hỗ trợ phát triển xương",               Price=220000,  Stock=80,  Category=ProductCategory.Food,      IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=4,  Name="Thức ăn Me-O cho mèo vị cá hồi",    Description="Thức ăn khô cho mèo trưởng thành vị cá hồi, giúp kiểm soát cân nặng hiệu quả",                   Price=185000,  Stock=150, Category=ProductCategory.Food,      IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── PHỤ KIỆN (Accessory) ──────────────────────────────────
            new Product { Id=5,  Name="Vòng cổ da cho chó",                 Description="Vòng cổ da bò thật, bền đẹp, khóa inox chống gỉ, nhiều màu sắc, điều chỉnh size linh hoạt",     Price=120000,  Stock=50,  Category=ProductCategory.Accessory, IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=6,  Name="Dây dắt chó tự co giãn 5m",         Description="Dây dắt tự co 5m, tay cầm chống trượt, khóa tự động an toàn, phù hợp chó dưới 25kg",            Price=220000,  Stock=60,  Category=ProductCategory.Accessory, IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=7,  Name="Lồng vận chuyển mèo size M",        Description="Lồng nhựa ABS cao cấp, thông thoáng, cửa inox chắc chắn, có tay xách và bánh xe tiện lợi",     Price=450000,  Stock=30,  Category=ProductCategory.Accessory, IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=8,  Name="Bát ăn inox 2 ngăn chống lật",      Description="Bát ăn inox 304 cao cấp 2 ngăn, đế cao su chống trượt, dung tích 400ml mỗi ngăn",               Price=95000,   Stock=120, Category=ProductCategory.Accessory, IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── THUỐC & CHĂM SÓC (Medicine) ──────────────────────────
            new Product { Id=9,  Name="Thuốc xổ giun cho chó mèo Drontal",  Description="Thuốc xổ giun phổ rộng an toàn, hiệu quả với nhiều loại ký sinh trùng đường ruột",              Price=65000,   Stock=80,  Category=ProductCategory.Medicine,  IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=10, Name="Nhỏ gáy trị ve rận Frontline Plus",   Description="Nhỏ gáy diệt ve rận, bọ chét hiệu quả 30 ngày, an toàn cho chó mèo từ 8 tuần tuổi",            Price=180000,  Stock=60,  Category=ProductCategory.Medicine,  IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── ĐỒ CHƠI (Toy) ─────────────────────────────────────────
            new Product { Id=11, Name="Bóng cao su có chuông cho chó",      Description="Bóng cao su tự nhiên bền, có chuông phát âm thanh, kích thích bản năng săn mồi của chó",         Price=75000,   Stock=90,  Category=ProductCategory.Toy,       IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=12, Name="Cần câu lông vũ cho mèo",            Description="Cần câu với lông vũ đầy màu sắc, giúp mèo vận động, giải trí và giảm stress hiệu quả",           Price=55000,   Stock=110, Category=ProductCategory.Toy,       IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=13, Name="Đồ chơi IQ nhét thức ăn cho chó",   Description="Đồ chơi nhựa nhét snack, kích thích trí tuệ, chống nhàm chán và ăn chậm cho chó",               Price=145000,  Stock=45,  Category=ProductCategory.Toy,       IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── CHẢI CỊT & LÀM ĐẸP (Grooming) ───────────────────────
            new Product { Id=14, Name="Lược chải lông chó mèo FURminator",  Description="Lược chải lông chuyên dụng, loại bỏ 90% lông rụng, giảm tình trạng bóng mao và lông bay khắp nhà", Price=280000, Stock=40, Category=ProductCategory.Grooming, IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=15, Name="Dầu gội chó mèo Bio-groom",          Description="Dầu gội thảo dược tự nhiên, pH cân bằng, làm sạch sâu, khử mùi hiệu quả, an toàn cho da nhạy cảm", Price=195000, Stock=70, Category=ProductCategory.Grooming, IsActive=true, CreatedAt=new DateTime(2024,1,1) }
        );

        modelBuilder.Entity<Pet>().HasData(
            // ── CHÓ ───────────────────────────────────────────────────
            new Pet { Id=1, Name="Buddy",   Breed="Golden Retriever",  Category=PetCategory.Dog,    AgeMonths=3,  Price=8500000,  Description="Chó Golden Retriever thuần chủng, ngoan hiền, lanh lợi, đã tiêm phòng đầy đủ 3 mũi",       Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=2, Name="Max",     Breed="Husky Siberian",    Category=PetCategory.Dog,    AgeMonths=4,  Price=12000000, Description="Husky Siberian thuần chủng, mắt xanh đẹp, lông dày, thích hợp khí hậu mát mẻ",            Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=3, Name="Coco",    Breed="Corgi Pembroke",    Category=PetCategory.Dog,    AgeMonths=3,  Price=9000000,  Description="Corgi chân ngắn dễ thương, thân thiện với trẻ em, thích vận động và chơi đùa",               Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=4, Name="Rocky",   Breed="Poodle Toy",        Category=PetCategory.Dog,    AgeMonths=2,  Price=6500000,  Description="Poodle Toy màu nâu sô-cô-la, thông minh, học lệnh nhanh, phù hợp sống trong căn hộ",       Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── MÈO ───────────────────────────────────────────────────
            new Pet { Id=5, Name="Mimi",    Breed="Mèo Anh lông ngắn", Category=PetCategory.Cat,    AgeMonths=2,  Price=4500000,  Description="Mèo Anh lông ngắn màu xám xanh, tính hiền lành, khỏe mạnh, không mắc bệnh di truyền",      Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=6, Name="Luna",    Breed="Mèo Scottish Fold", Category=PetCategory.Cat,    AgeMonths=3,  Price=7000000,  Description="Scottish Fold tai cụp siêu cute, lông ngắn màu trắng kem, cực kỳ thân thiện và ôn hòa",    Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── CHIM ──────────────────────────────────────────────────
            new Pet { Id=7, Name="Tweety",  Breed="Vẹt Cockatiel",     Category=PetCategory.Bird,   AgeMonths=6,  Price=1800000,  Description="Vẹt Cockatiel màu vàng xám, đã thuần, có thể học nói, hót hay, thích tương tác với người", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── THỎ ───────────────────────────────────────────────────
            new Pet { Id=8, Name="Snowy",   Breed="Thỏ Holland Lop",   Category=PetCategory.Rabbit, AgeMonths=4,  Price=2200000,  Description="Thỏ Holland Lop tai cụp màu trắng tuyết, hiền lành, sạch sẽ, thích ăn rau củ tươi",       Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) }
        );
    }
}
