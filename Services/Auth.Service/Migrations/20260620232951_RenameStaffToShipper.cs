using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Service.Migrations
{
    /// <inheritdoc />
    public partial class RenameStaffToShipper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$khnKDGa2/Gn3MOJmUk4qxOzFI9b3pMoiQknH8OHMstvr6BoGttP72");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "$2a$11$SOKbNKGzSN3ssjnhRQ7Fqu/yHBegGjIx7fVNFH3SUiRBn080meg16", "Staff" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$3Ew327ieY.mWvpph/uiUjOZUTe7jAerTs54NcMHSVjoqICWEMjytK");
        }
    }
}
