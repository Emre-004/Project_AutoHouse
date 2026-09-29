using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_AutoHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddCityToCarAd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Cars");
        }
    }
}
