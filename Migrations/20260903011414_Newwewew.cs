using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutor_Manager.Migrations
{
    /// <inheritdoc />
    public partial class Newwewew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAEAAYagAAAAEIvwxMAJ/t2/rwo6a30o39WIwMzC2nLntC98owiYso24zSx1fLtYAYjOcSRddprXUw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "Password@01Pass");
        }
    }
}
