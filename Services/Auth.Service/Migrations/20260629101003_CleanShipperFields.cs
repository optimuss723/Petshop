using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Service.Migrations
{
    /// <inheritdoc />
    public partial class CleanShipperFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$7b9i.EgAh1sCKrN3EJBLrOuvNaXy6wCYNgatnKbbGb72eU26jqxF2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "$2a$11$v0EaMpZkJZRWkfOkCQe7/ezOqsGwQsFyqzhFmP.2ZR5u4hcDOVh8O", "Staff" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$foovk5UpKXhpmLpfluejw.f51/qiDzfJmiRS14XDW1K2KlaMyovv6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ZUvKZNQQz1Op4.cOSv61COL6Ae0sYcBBNumtFDjMkx0mH/TepnvUe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "$2a$11$e/ltNH0McGDivFHcrw2zzuow2WOiJwCH1iH5vcnFKZQxKzqP57/qa", "Shipper" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$CJ.BNeDKZgHeLmbgW65F6uBlEDaTC88o43gbsp9dgvjyWWoAAZw7W");
        }
    }
}
