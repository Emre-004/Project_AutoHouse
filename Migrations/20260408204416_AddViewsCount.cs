using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_AutoHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddViewsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Views",
                table: "Cars");
        }
    }
}
