using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementSimulator.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecondManagers",
                columns: table => new
                {
                    SecondManagerEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReplacedManagerId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecondManagers", x => new { x.SecondManagerEmployeeId, x.ReplacedManagerId, x.StartDate });
                    table.ForeignKey(
                        name: "FK_SecondManagers_Users_ReplacedManagerId",
                        column: x => x.ReplacedManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecondManagers_Users_SecondManagerEmployeeId",
                        column: x => x.SecondManagerEmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecondManagers_ReplacedManagerId",
                table: "SecondManagers",
                column: "ReplacedManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecondManagers");
        }
    }
}
