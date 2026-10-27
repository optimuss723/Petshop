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
            new Product { Id=1,  Name="Thức ăn Royal Canin Maxi Adult cho chó", Description="Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein dễ tiêu hóa, omega-3 & EPA-DHA nuôi dưỡng lông mượt và bảo vệ xương khớp.", Price=350000, Stock=98, Category="Food", ImageUrl="https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=2,  Name="Thức ăn hạt Whiskas Vị Cá Biển cho mèo", Description="Thức ăn hạt cho mèo trưởng thành bổ sung Taurine sáng mắt, vitamin E tăng đề kháng và omega 6 cho da lông bóng khỏe.", Price=145000, Stock=200, Category="Food", ImageUrl="https://images.unsplash.com/photo-1568640347023-a616a30bc3bd?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=3,  Name="Vòng cổ da bò thật khóa inox cho chó", Description="Vòng cổ da bò thật 100%, đệm êm chống cọ xát da cổ, khóa bấm inox mạ chrome chống gỉ cực kỳ bền bỉ.", Price=120000, Stock=50, Category="Accessory", ImageUrl="https://images.unsplash.com/photo-1601758228041-f3b2795255f1?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=4,  Name="Cát vệ sinh đậu nành Cature tự nhiên cho mèo", Description="Cát đậu nành hữu cơ 100%, vón cục nhanh, kiểm soát mùi hôi vượt trội, tan hoàn toàn trong nước xả bồn cầu an toàn.", Price=85000, Stock=150, Category="Accessory", ImageUrl="https://images.unsplash.com/photo-1545249390-6bdfa286032f?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=5,  Name="Thuốc xổ giun phổ rộng Drontal Plus Bayer", Description="Thuốc tẩy giun dạng viên vị thịt bò thơm ngon, tiêu diệt hiệu quả giun đũa, giun móc, giun tóc và sán dây cho chó mèo.", Price=65000, Stock=80, Category="Medicine", ImageUrl="https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=6,  Name="Dây dắt tự co giãn 5m Flexi cao cấp", Description="Dây dắt tự co giãn 5 mét chịu lực chó dưới 25kg, hệ thống hãm phanh 1 nút bấm thông minh, tay cầm bọc cao su chống trượt.", Price=220000, Stock=60, Category="Accessory", ImageUrl="https://images.unsplash.com/photo-1535294435445-d7249524ef2e?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=7,  Name="Lồng vận chuyển thú cưng tiêu chuẩn hàng không Size M", Description="Lồng nhựa ABS nguyên sinh cao cấp chịu va đập cực tốt, cửa lồng thép sơn tĩnh điện chống rỉ, thiết kế thông gió 4 chiều.", Price=450000, Stock=30, Category="Accessory", ImageUrl="https://images.unsplash.com/photo-1548767797-d8c844163c4c?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=8,  Name="Bát ăn đôi Inox 304 chống trượt bảo vệ cột sống", Description="Bát ăn đôi inox không rỉ độ bền cao kèm khay nhựa nâng góc nghiêng 15 độ chống trào ngược thức ăn và bảo vệ đốt sống cổ thú cưng.", Price=95000, Stock=120, Category="Accessory", ImageUrl="https://images.unsplash.com/photo-1591946614720-90a587da4a36?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=9,  Name="Nhỏ gáy trị ve rận bọ chét Frontline Plus", Description="Thuốc nhỏ gáy diệt sạch ve rận, bọ chét và ấu trùng trong vòng 24-48 giờ, duy trì hiệu quả bảo vệ liên tục suốt 30 ngày.", Price=180000, Stock=60, Category="Medicine", ImageUrl="https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=10, Name="Bóng gặm cao su tự nhiên có chuông vui nhộn", Description="Bóng cao su thiên nhiên dai đàn hồi tốt, an toàn tuyệt đối khi cún cắn gặm, tích hợp chuông leng keng kích thích vận động.", Price=75000, Stock=90, Category="Toy", ImageUrl="https://images.unsplash.com/photo-1576201836106-db1758fd1c97?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=11, Name="Cần câu lông vũ gắn chuông giảm stress cho mèo", Description="Cần câu mèo cán đàn hồi mềm dẻo, đầu chùm lông vũ tự nhiên nhiều màu kèm chuông kích thích bản năng săn mồi của mèo cưng.", Price=55000, Stock=110, Category="Toy", ImageUrl="https://images.unsplash.com/photo-1533738363-b7f9aef128ce?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=12, Name="Đồ chơi gặm nhai sạch răng KONG Classic", Description="Đồ chơi cao su tự nhiên huyền thoại có khoang nhét pate/thức ăn thưởng, hỗ trợ làm sạch mảng bám răng và chống cắn phá đồ.", Price=145000, Stock=45, Category="Toy", ImageUrl="https://images.unsplash.com/photo-1587300003388-59208cc962cb?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=13, Name="Lược chải lông rụng chuyên dụng Furminator De-Shedding", Description="Lược thép răng mịn chuyên dụng triệt tiêu đến 90% lông chết và lông rụng, có nút bấm nhả lông tiện lợi, không gây rát da thú cưng.", Price=280000, Stock=40, Category="Grooming", ImageUrl="https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=14, Name="Sữa tắm thảo dược dưỡng lông siêu mượt Bio-Groom 355ml", Description="Sữa tắm thảo dược pH chuẩn 7.0 khử mùi hôi tận gốc, chiết xuất hoa cúc và lô hội dưỡng ẩm làm mềm mượt lông, hương thơm lưu giữ 7 ngày.", Price=195000, Stock=70, Category="Grooming", ImageUrl="https://images.unsplash.com/photo-1608248597359-bb47401a09d6?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) },
            new Product { Id=15, Name="Thức ăn Pedigree Pro Dinh Dưỡng Cho Chó Con", Description="Công thức giàu DHA & Choline phát triển trí não, lượng đạm và canxi tối ưu hỗ trợ cơ bắp và hệ xương phát triển khỏe mạnh.", Price=220000, Stock=85, Category="Food", ImageUrl="https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80", CreatedAt=new DateTime(2024,1,1) }
        );

        m.Entity<Pet>().HasData(
            new Pet { Id=1, Name="Chó Golden Retriever Vàng Kim Đực - Mã GD101", Breed="Golden Retriever", Category="Dog", AgeMonths=3, Price=8500000, Description="Chó Golden Retriever thuần chủng màu vàng kim, giới tính Đực, 3 tháng tuổi. Đã tiêm phòng 2 mũi vaccine phòng 7 bệnh có sổ tiêm theo dõi đầy đủ. Đã xổ giun 2 lần. Cún ăn khỏe, thân thiện, thông minh và cực kỳ quấn người.", ImageUrl="https://images.unsplash.com/photo-1552053831-71594a27632d?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=2, Name="Mèo Anh Lông Ngắn Xám Xanh Đực - Mã ALN104", Breed="Mèo Anh lông ngắn", Category="Cat", AgeMonths=3, Price=5200000, Description="Mèo Anh lông ngắn màu xám xanh thuần chủng, giới tính Đực, 3 tháng tuổi. Đầu to má bánh bao phúng phính, lông dày mượt như nhung. Đã tiêm 2 mũi vaccine phòng bệnh mèo và tẩy giun định kỳ. Rất quấn chủ và ngoan ngoãn.", ImageUrl="https://images.unsplash.com/photo-1574158622682-e40e69881006?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=3, Name="Chó Husky Siberian Mắt Xanh Đực - Mã HK102", Breed="Husky Siberian", Category="Dog", AgeMonths=4, Price=12000000, Description="Husky Siberian thuần chủng màu xám trắng, đôi mắt xanh biếc tuyệt đẹp, giới tính Đực, 4 tháng tuổi. Khung xương to chắc khỏe, bờm dày, đã tiêm phòng 2 mũi vaccine và kiểm tra sức khỏe âm tính Care/Parvo.", ImageUrl="https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=4, Name="Chó Corgi Pembroke Vàng Trắng Cái - Mã CG103", Breed="Corgi Pembroke", Category="Dog", AgeMonths=3, Price=9500000, Description="Chó Corgi Pembroke màu vàng trắng, chân siêu ngắn, mông to hình trái tim siêu đáng yêu, giới tính Cái, 3 tháng tuổi. Đã tiêm 2 mũi vaccine và tẩy giun định kỳ. Tính tình hoạt bát, thích chơi đùa và quấn chủ.", ImageUrl="https://images.unsplash.com/photo-1612536057832-2ff7ead58194?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=5, Name="Chó Poodle Tiny Nâu Đỏ Xoăn Cái - Mã PD107", Breed="Poodle Tiny", Category="Dog", AgeMonths=2, Price=6800000, Description="Poodle dòng Tiny siêu nhỏ gọn màu nâu đỏ ánh kim, giới tính Cái, 2.5 tháng tuổi. Lông xoăn bồng bềnh không rụng lông, rất phù hợp nuôi trong căn hộ. Đã tiêm đủ 2 mũi vaccine.", ImageUrl="https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=6, Name="Chó Shiba Inu Nhật Bản Vàng Trắng Đực - Mã SB105", Breed="Shiba Inu", Category="Dog", AgeMonths=3, Price=14000000, Description="Shiba Inu Nhật Bản thuần chủng màu vàng trắng, giới tính Đực, 3 tháng tuổi. Khuôn mặt cười duyên dáng, đuôi cuộn tròn, tai vểnh chuẩn giống. Đã tiêm phòng 2 mũi và gắn microchip định danh.", ImageUrl="https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=7, Name="Chó Bull Pháp Bò Sữa Đực - Mã BP108", Breed="Bull Pháp", Category="Dog", AgeMonths=3, Price=11500000, Description="Chó Bull Pháp (French Bulldog) màu bò sữa đen trắng, giới tính Đực, 3 tháng tuổi. Cơ bắp săn chắc, đầu to, tai dơi chuẩn đẹp, mặt xệ đáng yêu. Đã tiêm 2 mũi vaccine.", ImageUrl="https://images.unsplash.com/photo-1583512603805-3cc6b41f3edb?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) },
            new Pet { Id=8, Name="Mèo Scottish Fold Tai Cụp Trắng Kem Cái - Mã SF106", Breed="Scottish Fold", Category="Cat", AgeMonths=3, Price=7500000, Description="Mèo Scottish Fold tai cụp sát đầu tròn xoe, màu trắng kem phấn, giới tính Cái, 3 tháng tuổi. Tính cách dịu dàng, thích vuốt ve, đã được tẩy giun và tiêm vaccine 2 mũi kèm sổ theo dõi.", ImageUrl="https://images.unsplash.com/photo-1573865526739-10659fec78a5?auto=format&fit=crop&w=800&q=80", Status="Available", CreatedAt=new DateTime(2024,1,1) }
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
