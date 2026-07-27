using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRichSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgeMonths = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    PetId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "AgeMonths", "Breed", "Category", "CreatedAt", "Description", "ImageUrl", "Name", "Price", "Status" },
                values: new object[,]
                {
                    { 1, 3, "Golden Retriever", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chó Golden Retriever thuần chủng, ngoan hiền, lanh lợi, đã tiêm phòng đầy đủ 3 mũi", null, "Buddy", 8500000m, "Available" },
                    { 2, 4, "Husky Siberian", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Husky Siberian thuần chủng, mắt xanh đẹp, lông dày, thích hợp khí hậu mát mẻ", null, "Max", 12000000m, "Available" },
                    { 3, 3, "Corgi Pembroke", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Corgi chân ngắn dễ thương, thân thiện với trẻ em, thích vận động và chơi đùa", null, "Coco", 9000000m, "Available" },
                    { 4, 2, "Poodle Toy", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Poodle Toy màu nâu sô-cô-la, thông minh, học lệnh nhanh, phù hợp sống trong căn hộ", null, "Rocky", 6500000m, "Available" },
                    { 5, 2, "Mèo Anh lông ngắn", "Cat", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mèo Anh lông ngắn màu xám xanh, tính hiền lành, khỏe mạnh, không mắc bệnh di truyền", null, "Mimi", 4500000m, "Available" },
                    { 6, 3, "Mèo Scottish Fold", "Cat", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Scottish Fold tai cụp siêu cute, lông ngắn màu trắng kem, cực kỳ thân thiện và ôn hòa", null, "Luna", 7000000m, "Available" },
                    { 7, 6, "Vẹt Cockatiel", "Bird", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vẹt Cockatiel màu vàng xám, đã thuần, có thể học nói, hót hay, thích tương tác với người", null, "Tweety", 1800000m, "Available" },
                    { 8, 4, "Thỏ Holland Lop", "Rabbit", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thỏ Holland Lop tai cụp màu trắng tuyết, hiền lành, sạch sẽ, thích ăn rau củ tươi", null, "Snowy", 2200000m, "Available" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein và omega-3, giúp lông bóng mượt", null, true, "Thức ăn Royal Canin cho chó", 350000m, 100 },
                    { 2, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn ướt cho mèo mọi lứa tuổi, hương cá ngừ thơm ngon, bổ sung taurine tốt cho mắt", null, true, "Thức ăn Whiskas cho mèo", 45000m, 200 },
                    { 3, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn chuyên dụng cho chó con dưới 1 tuổi, giàu canxi hỗ trợ phát triển xương", null, true, "Thức ăn Pedigree cho chó con", 220000m, 80 },
                    { 4, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn khô cho mèo trưởng thành vị cá hồi, giúp kiểm soát cân nặng hiệu quả", null, true, "Thức ăn Me-O cho mèo vị cá hồi", 185000m, 150 },
                    { 5, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vòng cổ da bò thật, bền đẹp, khóa inox chống gỉ, nhiều màu sắc, điều chỉnh size linh hoạt", null, true, "Vòng cổ da cho chó", 120000m, 50 },
                    { 6, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dây dắt tự co 5m, tay cầm chống trượt, khóa tự động an toàn, phù hợp chó dưới 25kg", null, true, "Dây dắt chó tự co giãn 5m", 220000m, 60 },
                    { 7, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lồng nhựa ABS cao cấp, thông thoáng, cửa inox chắc chắn, có tay xách và bánh xe tiện lợi", null, true, "Lồng vận chuyển mèo size M", 450000m, 30 },
                    { 8, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bát ăn inox 304 cao cấp 2 ngăn, đế cao su chống trượt, dung tích 400ml mỗi ngăn", null, true, "Bát ăn inox 2 ngăn chống lật", 95000m, 120 },
                    { 9, "Medicine", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thuốc xổ giun phổ rộng an toàn, hiệu quả với nhiều loại ký sinh trùng đường ruột", null, true, "Thuốc xổ giun cho chó mèo Drontal", 65000m, 80 },
                    { 10, "Medicine", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nhỏ gáy diệt ve rận, bọ chét hiệu quả 30 ngày, an toàn cho chó mèo từ 8 tuần tuổi", null, true, "Nhỏ gáy trị ve rận Frontline Plus", 180000m, 60 },
                    { 11, "Toy", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bóng cao su tự nhiên bền, có chuông phát âm thanh, kích thích bản năng săn mồi của chó", null, true, "Bóng cao su có chuông cho chó", 75000m, 90 },
                    { 12, "Toy", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cần câu với lông vũ đầy màu sắc, giúp mèo vận động, giải trí và giảm stress hiệu quả", null, true, "Cần câu lông vũ cho mèo", 55000m, 110 },
                    { 13, "Toy", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đồ chơi nhựa nhét snack, kích thích trí tuệ, chống nhàm chán và ăn chậm cho chó", null, true, "Đồ chơi IQ nhét thức ăn cho chó", 145000m, 45 },
                    { 14, "Grooming", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lược chải lông chuyên dụng, loại bỏ 90% lông rụng, giảm tình trạng bóng mao và lông bay khắp nhà", null, true, "Lược chải lông chó mèo FURminator", 280000m, 40 },
                    { 15, "Grooming", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dầu gội thảo dược tự nhiên, pH cân bằng, làm sạch sâu, khử mùi hiệu quả, an toàn cho da nhạy cảm", null, true, "Dầu gội chó mèo Bio-groom", 195000m, 70 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Phone", "RefreshToken", "RefreshTokenExpiry", "Role" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@petshop.com", "Admin PetShop", true, "$2a$11$d4albRJBE0WOElKVytubBuY7KFmxaJkAb7gzItnEkMhPYYa7rjxka", "0901234567", null, null, "Admin" },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "staff@petshop.com", "Nhân viên 1", true, "$2a$11$j3k.qf0VWxYImYMbEYoQNux.rq/wq0b6XFYhLA6rU8.2vYCJT8YBy", "0901234568", null, null, "Staff" },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer@petshop.com", "Nguyễn Văn A", true, "$2a$11$r983lwoYVThLpwP179Vxu.kMyRR275KOLC1qEMwtqW2CuXPUGlA6y", "0901234569", null, null, "Customer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PetId",
                table: "OrderItems",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
