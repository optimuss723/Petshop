using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    PetId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Icon", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "Thức ăn hạt, ướt cho các loài thú cưng", "🍖", "Thức ăn", "Product" },
                    { 2, "Vòng cổ, dây xích, lồng, đệm nằm...", "🎀", "Phụ kiện", "Product" },
                    { 3, "Thuốc xổ giun, vaccine, vitamin...", "💊", "Thuốc", "Product" },
                    { 4, "Đồ chơi tương tác, bóng, cần câu cho thú cưng", "🎾", "Đồ chơi", "Product" },
                    { 5, "Các giống chó đang bán tại cửa hàng", "🐕", "Chó", "Pet" },
                    { 6, "Các giống mèo đang bán tại cửa hàng", "🐈", "Mèo", "Pet" },
                    { 7, "Các loài chim đang bán tại cửa hàng", "🐦", "Chim", "Pet" },
                    { 8, "Các loài cá cảnh đang bán", "🐟", "Cá", "Pet" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
