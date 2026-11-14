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
            new Product { Id=1,  Name="Thức ăn Royal Canin cho chó",       Description="Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein và omega-3, giúp lông bóng mượt",          Price=350000,  Stock=100, Category=ProductCategory.Food,      ImageUrl="https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=2,  Name="Thức ăn Whiskas cho mèo",            Description="Thức ăn ướt cho mèo mọi lứa tuổi, hương cá ngừ thơm ngon, bổ sung taurine tốt cho mắt",          Price=45000,   Stock=200, Category=ProductCategory.Food,      ImageUrl="https://images.unsplash.com/photo-1568640347023-a616a30bc3bd?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=3,  Name="Thức ăn Pedigree cho chó con",       Description="Thức ăn chuyên dụng cho chó con dưới 1 tuổi, giàu canxi hỗ trợ phát triển xương",               Price=220000,  Stock=80,  Category=ProductCategory.Food,      ImageUrl="https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=4,  Name="Thức ăn Me-O cho mèo vị cá hồi",    Description="Thức ăn khô cho mèo trưởng thành vị cá hồi, giúp kiểm soát cân nặng hiệu quả",                   Price=185000,  Stock=150, Category=ProductCategory.Food,      ImageUrl="https://images.unsplash.com/photo-1545249390-6bdfa286032f?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── PHỤ KIỆN (Accessory) ──────────────────────────────────
            new Product { Id=5,  Name="Vòng cổ da cho chó",                 Description="Vòng cổ da bò thật, bền đẹp, khóa inox chống gỉ, nhiều màu sắc, điều chỉnh size linh hoạt",     Price=120000,  Stock=50,  Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1601758228041-f3b2795255f1?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=6,  Name="Dây dắt chó tự co giãn 5m",         Description="Dây dắt tự co 5m, tay cầm chống trượt, khóa tự động an toàn, phù hợp chó dưới 25kg",            Price=220000,  Stock=60,  Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1535294435445-d7249524ef2e?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=7,  Name="Lồng vận chuyển mèo size M",        Description="Lồng nhựa ABS cao cấp, thông thoáng, cửa inox chắc chắn, có tay xách và bánh xe tiện lợi",     Price=450000,  Stock=30,  Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1548767797-d8c844163c4c?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=8,  Name="Bát ăn inox 2 ngăn chống lật",      Description="Bát ăn inox 304 cao cấp 2 ngăn, đế cao su chống trượt, dung tích 400ml mỗi ngăn",               Price=95000,   Stock=120, Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1591946614720-90a587da4a36?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── THUỐC & CHĂM SÓC (Medicine) ──────────────────────────
            new Product { Id=9,  Name="Thuốc xổ giun cho chó mèo Drontal",  Description="Thuốc xổ giun phổ rộng an toàn, hiệu quả với nhiều loại ký sinh trùng đường ruột",              Price=65000,   Stock=80,  Category=ProductCategory.Medicine,  ImageUrl="https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=10, Name="Nhỏ gáy trị ve rận Frontline Plus",   Description="Nhỏ gáy diệt ve rận, bọ chét hiệu quả 30 ngày, an toàn cho chó mèo từ 8 tuần tuổi",            Price=180000,  Stock=60,  Category=ProductCategory.Medicine,  ImageUrl="https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── ĐỒ CHƠI (Toy) ─────────────────────────────────────────
            new Product { Id=11, Name="Bóng cao su có chuông cho chó",      Description="Bóng cao su tự nhiên bền, có chuông phát âm thanh, kích thích bản năng săn mồi của chó",         Price=75000,   Stock=90,  Category=ProductCategory.Toy,       ImageUrl="https://images.unsplash.com/photo-1576201836106-db1758fd1c97?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=12, Name="Cần câu lông vũ cho mèo",            Description="Cần câu với lông vũ đầy màu sắc, giúp mèo vận động, giải trí và giảm stress hiệu quả",           Price=55000,   Stock=110, Category=ProductCategory.Toy,       ImageUrl="https://images.unsplash.com/photo-1533738363-b7f9aef128ce?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=13, Name="Đồ chơi IQ nhét thức ăn cho chó",   Description="Đồ chơi nhựa nhét snack, kích thích trí tuệ, chống nhàm chán và ăn chậm cho chó",               Price=145000,  Stock=45,  Category=ProductCategory.Toy,       ImageUrl="https://images.unsplash.com/photo-1587300003388-59208cc962cb?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            // ── CHẢI CỊT & LÀM ĐẸP (Grooming) ───────────────────────
            new Product { Id=14, Name="Lược chải lông chó mèo FURminator",  Description="Lược chải lông chuyên dụng, loại bỏ 90% lông rụng, giảm tình trạng bóng mao và lông bay khắp nhà", Price=280000, Stock=40, Category=ProductCategory.Grooming,   ImageUrl="https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=15, Name="Dầu gội chó mèo Bio-groom",          Description="Dầu gội thảo dược tự nhiên, pH cân bằng, làm sạch sâu, khử mùi hiệu quả, an toàn cho da nhạy cảm", Price=195000, Stock=70, Category=ProductCategory.Grooming,   ImageUrl="https://images.unsplash.com/photo-1608248597359-bb47401a09d6?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=16, Name="Pate Mèo Ciao Churu vị ức gà & cá ngừ", Description="Pate dạng kem thơm ngon bổ dưỡng cho mèo mọi lứa tuổi, kích thích ăn uống ngon miệng.", Price=85000, Stock=250, Category=ProductCategory.Food, ImageUrl="https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=17, Name="Xương gặm sạch răng Dental Canxi", Description="Xương nhai làm sạch mảng bám trên răng cho chó, chống hôi miệng và bổ sung canxi.", Price=75000, Stock=140, Category=ProductCategory.Food, ImageUrl="https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=18, Name="Trụ cào móng mèo 3 tầng bọc dây thừng", Description="Trụ cào móng tích hợp nhà cây 3 tầng vững chãi, bọc dây sisal tự nhiên bền bỉ chống cào phá đồ.", Price=320000, Stock=35, Category=ProductCategory.Toy, ImageUrl="https://images.unsplash.com/photo-1545249390-6bdfa286032f?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=19, Name="Đệm tròn lông cừu êm ái chó mèo size L", Description="Đệm nằm siêu mềm mại ấm áp, bông PP đàn hồi cao cấp, đáy chống trượt và chống thấm ẩm.", Price=260000, Stock=40, Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1541599540903-216a46ca1dc0?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=20, Name="Máy lọc nước tự động cho thú cưng 2.5L", Description="Máy uống nước tuần hoàn tự động kèm màng lọc ion đa tầng, siêu êm, kích thích thú cưng uống nước.", Price=380000, Stock=55, Category=ProductCategory.Accessory, ImageUrl="https://images.unsplash.com/photo-1591946614720-90a587da4a36?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=21, Name="Gel dinh dưỡng Megaderm mượt lông", Description="Gel bổ sung axit béo thiết yếu Omega 3 & 6, giảm rụng lông, phục hồi da nhạy cảm dị ứng.", Price=215000, Stock=65, Category=ProductCategory.Medicine, ImageUrl="https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=800&q=80", IsActive=true, CreatedAt=new DateTime(2024,1,1) }
        );

        modelBuilder.Entity<Pet>().HasData(
            // ── CHÓ ───────────────────────────────────────────────────
            new Pet { Id=1, Name="Buddy",   Breed="Golden Retriever",  Category=PetCategory.Dog,    AgeMonths=3,  Price=8500000,  Description="Chó Golden Retriever thuần chủng, ngoan hiền, lanh lợi, đã tiêm phòng đầy đủ 3 mũi",       ImageUrl="https://images.unsplash.com/photo-1552053831-71594a27632d?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=2, Name="Max",     Breed="Husky Siberian",    Category=PetCategory.Dog,    AgeMonths=4,  Price=12000000, Description="Husky Siberian thuần chủng, mắt xanh đẹp, lông dày, thích hợp khí hậu mát mẻ",            ImageUrl="https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=3, Name="Coco",    Breed="Corgi Pembroke",    Category=PetCategory.Dog,    AgeMonths=3,  Price=9000000,  Description="Corgi chân ngắn dễ thương, thân thiện với trẻ em, thích vận động và chơi đùa",               ImageUrl="https://images.unsplash.com/photo-1612536057832-2ff7ead58194?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=4, Name="Rocky",   Breed="Poodle Toy",        Category=PetCategory.Dog,    AgeMonths=2,  Price=6500000,  Description="Poodle Toy màu nâu sô-cô-la, thông minh, học lệnh nhanh, phù hợp sống trong căn hộ",       ImageUrl="https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=9, Name="Lucky",   Breed="Shiba Inu",         Category=PetCategory.Dog,    AgeMonths=4,  Price=11500000, Description="Shiba Inu Nhật Bản thuần chủng, mặt cười đáng yêu, lanh lợi, tính cách độc lập và trung thành tuyệt đối.", ImageUrl="https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=10, Name="Mochi",  Breed="Samoyed Tuyết Trắng", Category=PetCategory.Dog,  AgeMonths=3,  Price=13500000, Description="Samoyed lông trắng như tuyết, nụ cười thiên thần, cực kỳ quấn chủ và thân thiện với trẻ nhỏ.", ImageUrl="https://images.unsplash.com/photo-1529429617124-95b109e86bb8?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=11, Name="Bino",   Breed="Pug Mặt Xệ",        Category=PetCategory.Dog,    AgeMonths=2,  Price=5500000,  Description="Pug mặt xệ màu vàng kim, đôi mắt tròn xoe ngây thơ, tính tình hiền lành, thích được ôm ấp.", ImageUrl="https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=12, Name="Rex",    Breed="Becgie Đức (GSD)",  Category=PetCategory.Dog,    AgeMonths=4,  Price=9800000,  Description="Chó chăn cừu Đức thông minh đỉnh cao, vóc dáng vạm vỡ, can đảm, thích hợp huấn luyện bảo vệ.", ImageUrl="https://images.unsplash.com/photo-1589941013453-ec89f33b5e95?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── MÈO ───────────────────────────────────────────────────
            new Pet { Id=5, Name="Mimi",    Breed="Mèo Anh lông ngắn", Category=PetCategory.Cat,    AgeMonths=2,  Price=4500000,  Description="Mèo Anh lông ngắn màu xám xanh, tính hiền lành, khỏe mạnh, không mắc bệnh di truyền",      ImageUrl="https://images.unsplash.com/photo-1574158622682-e40e69881006?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=6, Name="Luna",    Breed="Mèo Scottish Fold", Category=PetCategory.Cat,    AgeMonths=3,  Price=7000000,  Description="Scottish Fold tai cụp siêu cute, lông ngắn màu trắng kem, cực kỳ thân thiện và ôn hòa",    ImageUrl="https://images.unsplash.com/photo-1573865526739-10659fec78a5?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=13, Name="Bông",   Breed="Mèo Ba Tư Persian", Category=PetCategory.Cat,    AgeMonths=3,  Price=6800000,  Description="Mèo Ba Tư lông xù quý tộc, mặt tịt đáng yêu, tính cách trầm tĩnh, dịu dàng, thích vuốt ve.", ImageUrl="https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=14, Name="Simba",  Breed="Mèo Bengal Vân Báo", Category=PetCategory.Cat,   AgeMonths=3,  Price=15000000, Description="Mèo Bengal hoa văn da báo hoang dã, thông minh, năng động, bơi lội giỏi, thần thái quý phái.", ImageUrl="https://images.unsplash.com/photo-1513360309081-38f076278f1d?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=15, Name="Kiki",   Breed="Mèo Xiêm Siamese",  Category=PetCategory.Cat,    AgeMonths=4,  Price=5200000,  Description="Mèo Xiêm mắt xanh biếc hút hồn, vóc dáng thanh mảnh kiêu sa, thông minh và thích giao tiếp.", ImageUrl="https://images.unsplash.com/photo-1535930891776-0c2dfb7fda1a?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=16, Name="Nấm",    Breed="Mèo Munchkin Chân Ngắn", Category=PetCategory.Cat, AgeMonths=2, Price=10500000, Description="Mèo Munchkin 4 chân ngắn ngủn lon ton, lông màu bicolor, tính cách nhí nhảnh vô cùng dễ thương.", ImageUrl="https://images.unsplash.com/photo-1561948955-570b270e7c36?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── CHIM ──────────────────────────────────────────────────
            new Pet { Id=7, Name="Tweety",  Breed="Vẹt Cockatiel",     Category=PetCategory.Bird,   AgeMonths=6,  Price=1800000,  Description="Vẹt Cockatiel màu vàng xám, đã thuần, có thể học nói, hót hay, thích tương tác với người", ImageUrl="https://images.unsplash.com/photo-1552728089-57bdde30beb3?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=17, Name="Rio",    Breed="Vẹt Macaw Nam Mỹ",  Category=PetCategory.Bird,   AgeMonths=8,  Price=25000000, Description="Vẹt Macaw Nam Mỹ tuyệt đẹp, thông minh vượt trội, đã học được hơn 10 từ tiếng Việt, sống lâu tới 50 năm.", ImageUrl="https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=18, Name="Chirpy", Breed="Vẹt Yến Phụng Budgie", Category=PetCategory.Bird, AgeMonths=3, Price=4500000, Description="Vẹt Yến Phụng lông xanh ngọc bích, nhỏ nhắn nhanh nhẹn, hót líu lo cả ngày mang lại niềm vui cho gia đình.", ImageUrl="https://images.unsplash.com/photo-1506220926639-b5ba770e0501?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── THỎ ───────────────────────────────────────────────────
            new Pet { Id=8, Name="Snowy",   Breed="Thỏ Holland Lop",   Category=PetCategory.Rabbit, AgeMonths=4,  Price=2200000,  Description="Thỏ Holland Lop tai cụp màu trắng tuyết, hiền lành, sạch sẽ, thích ăn rau củ tươi",       ImageUrl="https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=21, Name="Cottontail", Breed="Thỏ Minilop Thuần Chủng", Category=PetCategory.Rabbit, AgeMonths=3, Price=1600000, Description="Thỏ Minilop tai cụp màu xám khói, bộ lông tơ mềm mượt như nhung, tính cách hiền hòa và sạch sẽ.", ImageUrl="https://images.unsplash.com/photo-1518796745738-41048802f99a?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            // ── CÁ ────────────────────────────────────────────────────
            new Pet { Id=19, Name="Ruby",   Breed="Cá La Hán Kim Cương", Category=PetCategory.Fish, AgeMonths=5, Price=3200000, Description="Cá La Hán gù to tròn, châu sáng lấp lánh như kim cương, dáng bơi uy nghiêm, biểu tượng phong thủy tài lộc.", ImageUrl="https://images.unsplash.com/photo-1522069169874-c58ec4b76be5?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=20, Name="Goldie", Breed="Cá Betta Halfmoon Dumbo", Category=PetCategory.Fish, AgeMonths=4, Price=350000, Description="Cá xiêm đá Betta đuôi xòe 180 độ Halfmoon, tai bơi Dumbo to rộng, sắc đỏ ánh kim rực rỡ tuyệt mỹ.", ImageUrl="https://images.unsplash.com/photo-1534447677768-be436bb09401?auto=format&fit=crop&w=800&q=80", Status=PetStatus.Available, CreatedAt=new DateTime(2024,1,1) }
        );
    }
}
