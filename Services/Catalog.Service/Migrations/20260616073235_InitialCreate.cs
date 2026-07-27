using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AgeMonths = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "AgeMonths", "Breed", "Category", "CreatedAt", "Description", "ImageUrl", "Name", "Price", "Status" },
                values: new object[,]
                {
                    { 1, 3, "Golden Retriever", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chó Golden Retriever thuần chủng, đã tiêm phòng", null, "Buddy", 8500000m, "Available" },
                    { 2, 2, "Mèo Anh lông ngắn", "Cat", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mèo Anh lông ngắn màu xám xanh, khỏe mạnh", null, "Mimi", 4500000m, "Available" },
                    { 3, 4, "Husky Siberian", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Husky Siberian thuần chủng, mắt xanh đẹp", null, "Max", 12000000m, "Available" },
                    { 4, 3, "Corgi Pembroke", "Dog", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Corgi dễ thương, thân thiện với trẻ em", null, "Coco", 9000000m, "Available" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn hạt cao cấp cho chó trưởng thành", null, true, "Thức ăn Royal Canin cho chó", 350000m, 100 },
                    { 2, "Food", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thức ăn ướt cho mèo mọi lứa tuổi", null, true, "Thức ăn Whiskas cho mèo", 45000m, 200 },
                    { 3, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vòng cổ da bền đẹp, nhiều màu sắc", null, true, "Vòng cổ da cho chó", 120000m, 50 },
                    { 4, "Accessory", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cát vệ sinh khử mùi hiệu quả", null, true, "Cát vệ sinh cho mèo", 85000m, 150 },
                    { 5, "Medicine", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thuốc xổ giun an toàn, hiệu quả", null, true, "Thuốc xổ giun cho chó mèo", 65000m, 80 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
