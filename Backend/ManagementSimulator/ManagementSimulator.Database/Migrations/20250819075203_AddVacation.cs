using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementSimulator.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddVacation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Vacation",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 21);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vacation",
                table: "Users");
        }
    }
}
