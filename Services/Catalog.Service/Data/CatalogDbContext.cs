using Catalog.Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Data;

/// <summary>
/// DbContext cho Catalog.Service.
/// Truy cập Products, Pets, Reviews, Categories — độc lập với các service khác.
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product>  Products   { get; set; }
    public DbSet<Pet>      Pets       { get; set; }
    public DbSet<Review>   Reviews    { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasMaxLength(50);
            e.Property(p => p.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Pet>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasMaxLength(50);
            e.Property(p => p.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.Property(r => r.Rating).IsRequired();
            e.Property(r => r.Comment).HasMaxLength(1000);
            e.Property(r => r.UserName).HasMaxLength(200);
            e.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Type).HasMaxLength(20);
            e.Property(c => c.Icon).HasMaxLength(20);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder m)
    {
        m.Entity<Product>().HasData(
            // ── THỨC ĂN ──────────────────────────────────────────────
            new Product { Id=1,  Name="Thức ăn Royal Canin cho chó",     Description="Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein và omega-3",    Price=350000, Stock=100, Category="Food",      CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=2,  Name="Thức ăn Whiskas cho mèo",          Description="Thức ăn ướt cho mèo mọi lứa tuổi, hương cá ngừ, bổ sung taurine",     Price=45000,  Stock=200, Category="Food",      CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=3,  Name="Thức ăn Pedigree cho chó con",     Description="Thức ăn chuyên dụng cho chó con dưới 1 tuổi, giàu canxi",              Price=220000, Stock=80,  Category="Food",      CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=4,  Name="Thức ăn Me-O cho mèo vị cá hồi",  Description="Thức ăn khô cho mèo trưởng thành vị cá hồi, kiểm soát cân nặng",      Price=185000, Stock=150, Category="Food",      CreatedAt=new DateTime(2024,1,1) },
            // ── PHỤ KIỆN ─────────────────────────────────────────────
            new Product { Id=5,  Name="Vòng cổ da cho chó",               Description="Vòng cổ da bò thật, khóa inox chống gỉ, nhiều màu sắc",               Price=120000, Stock=50,  Category="Accessory", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=6,  Name="Dây dắt chó tự co giãn 5m",       Description="Dây dắt tự co 5m, tay cầm chống trượt, phù hợp chó dưới 25kg",        Price=220000, Stock=60,  Category="Accessory", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=7,  Name="Lồng vận chuyển mèo size M",      Description="Lồng nhựa ABS cao cấp, thông thoáng, cửa inox chắc chắn",             Price=450000, Stock=30,  Category="Accessory", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=8,  Name="Bát ăn inox 2 ngăn chống lật",    Description="Bát ăn inox 304, đế cao su chống trượt, dung tích 400ml mỗi ngăn",    Price=95000,  Stock=120, Category="Accessory", CreatedAt=new DateTime(2024,1,1) },
            // ── THUỐC ────────────────────────────────────────────────
            new Product { Id=9,  Name="Thuốc xổ giun Drontal",            Description="Thuốc xổ giun phổ rộng an toàn, hiệu quả với nhiều loại ký sinh trùng",Price=65000,  Stock=80,  Category="Medicine",  CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=10, Name="Nhỏ gáy Frontline Plus",           Description="Nhỏ gáy diệt ve rận, bọ chét hiệu quả 30 ngày",                       Price=180000, Stock=60,  Category="Medicine",  CreatedAt=new DateTime(2024,1,1) },
            // ── ĐỒ CHƠI ──────────────────────────────────────────────
            new Product { Id=11, Name="Bóng cao su có chuông cho chó",    Description="Bóng cao su tự nhiên bền, chuông phát âm, kích thích bản năng săn mồi",Price=75000,  Stock=90,  Category="Toy",       CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=12, Name="Cần câu lông vũ cho mèo",          Description="Cần câu lông vũ đầy màu sắc, giúp mèo vận động và giảm stress",       Price=55000,  Stock=110, Category="Toy",       CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=13, Name="Đồ chơi IQ nhét thức ăn cho chó", Description="Đồ chơi nhựa nhét snack, kích thích trí tuệ, chống ăn nhanh",         Price=145000, Stock=45,  Category="Toy",       CreatedAt=new DateTime(2024,1,1) },
            // ── GROOMING ─────────────────────────────────────────────
            new Product { Id=14, Name="Lược chải lông FURminator",        Description="Lược chải lông chuyên dụng, loại bỏ 90% lông rụng",                   Price=280000, Stock=40,  Category="Grooming",  CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=15, Name="Dầu gội chó mèo Bio-groom",       Description="Dầu gội thảo dược tự nhiên, pH cân bằng, khử mùi hiệu quả",           Price=195000, Stock=70,  Category="Grooming",  CreatedAt=new DateTime(2024,1,1) }
        );

        m.Entity<Pet>().HasData(
            new Pet { Id=1, Name="Buddy",  Breed="Golden Retriever",  Category="Dog",    AgeMonths=3, Price=8500000,  Description="Chó Golden Retriever thuần chủng, ngoan hiền, đã tiêm phòng đầy đủ",      Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=2, Name="Max",    Breed="Husky Siberian",    Category="Dog",    AgeMonths=4, Price=12000000, Description="Husky Siberian thuần chủng, mắt xanh đẹp, lông dày",                    Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=3, Name="Coco",   Breed="Corgi Pembroke",    Category="Dog",    AgeMonths=3, Price=9000000,  Description="Corgi chân ngắn dễ thương, thân thiện với trẻ em",                       Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=4, Name="Rocky",  Breed="Poodle Toy",        Category="Dog",    AgeMonths=2, Price=6500000,  Description="Poodle Toy màu nâu, thông minh, phù hợp sống trong căn hộ",             Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=5, Name="Mimi",   Breed="Mèo Anh lông ngắn", Category="Cat",    AgeMonths=2, Price=4500000,  Description="Mèo Anh lông ngắn màu xám xanh, tính hiền lành, khỏe mạnh",             Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=6, Name="Luna",   Breed="Scottish Fold",     Category="Cat",    AgeMonths=3, Price=7000000,  Description="Scottish Fold tai cụp siêu cute, lông trắng kem, thân thiện",            Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=7, Name="Tweety", Breed="Vẹt Cockatiel",     Category="Bird",   AgeMonths=6, Price=1800000,  Description="Vẹt Cockatiel đã thuần, có thể học nói, hót hay",                        Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=8, Name="Snowy",  Breed="Thỏ Holland Lop",   Category="Rabbit", AgeMonths=4, Price=2200000,  Description="Thỏ Holland Lop tai cụp trắng tuyết, hiền lành, sạch sẽ",               Status="Available", CreatedAt=new DateTime(2024,1,1) }
        );

        m.Entity<Category>().HasData(
            new Category { Id=1, Name="Thức ăn",  Type="Product", Description="Thức ăn hạt, ướt cho các loài thú cưng",        Icon="🍖" },
            new Category { Id=2, Name="Phụ kiện", Type="Product", Description="Vòng cổ, dây xích, lồng, đệm nằm...",           Icon="🎀" },
            new Category { Id=3, Name="Thuốc",    Type="Product", Description="Thuốc xổ giun, vaccine, vitamin...",             Icon="💊" },
            new Category { Id=4, Name="Đồ chơi",  Type="Product", Description="Đồ chơi tương tác, bóng, cần câu cho thú cưng", Icon="🎾" },
            new Category { Id=5, Name="Grooming", Type="Product", Description="Lược chải, dầu gội, kéo cắt tỉa lông",          Icon="✂️" },
            new Category { Id=6, Name="Chó",      Type="Pet",     Description="Các giống chó đang bán tại cửa hàng",            Icon="🐕" },
            new Category { Id=7, Name="Mèo",      Type="Pet",     Description="Các giống mèo đang bán tại cửa hàng",            Icon="🐈" },
            new Category { Id=8, Name="Chim",     Type="Pet",     Description="Các loài chim đang bán tại cửa hàng",            Icon="🐦" },
            new Category { Id=9, Name="Thỏ",      Type="Pet",     Description="Các loài thỏ cảnh đang bán",                    Icon="🐰" }
        );
    }
}
