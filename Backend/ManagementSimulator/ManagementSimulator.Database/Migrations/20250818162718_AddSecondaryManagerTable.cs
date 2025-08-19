using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementSimulator.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryManagerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecondaryManagers",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SecondaryManagerId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByAdminId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecondaryManagers", x => new { x.EmployeeId, x.SecondaryManagerId, x.StartDate });
                    table.ForeignKey(
                        name: "FK_SecondaryManagers_Users_AssignedByAdminId",
                        column: x => x.AssignedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecondaryManagers_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecondaryManagers_Users_SecondaryManagerId",
                        column: x => x.SecondaryManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryManagers_AssignedByAdminId",
                table: "SecondaryManagers",
                column: "AssignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryManagers_SecondaryManagerId",
                table: "SecondaryManagers",
                column: "SecondaryManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecondaryManagers");
        }
    }
}
