using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementSimulator.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSecondaryManagerInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecondaryManagers_Users_AssignedByAdminId",
                table: "SecondaryManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_SecondaryManagers_Users_SecondaryManagerId",
                table: "SecondaryManagers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecondaryManagers",
                table: "SecondaryManagers");

            migrationBuilder.DropIndex(
                name: "IX_SecondaryManagers_SecondaryManagerId",
                table: "SecondaryManagers");

            migrationBuilder.DropColumn(
                name: "SecondaryManagerId",
                table: "SecondaryManagers");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "SecondaryManagers");

            migrationBuilder.RenameColumn(
                name: "AssignedByAdminId",
                table: "SecondaryManagers",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_SecondaryManagers_AssignedByAdminId",
                table: "SecondaryManagers",
                newName: "IX_SecondaryManagers_ManagerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecondaryManagers",
                table: "SecondaryManagers",
                columns: new[] { "EmployeeId", "ManagerId", "StartDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_SecondaryManagers_Users_ManagerId",
                table: "SecondaryManagers",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecondaryManagers_Users_ManagerId",
                table: "SecondaryManagers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecondaryManagers",
                table: "SecondaryManagers");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "SecondaryManagers",
                newName: "AssignedByAdminId");

            migrationBuilder.RenameIndex(
                name: "IX_SecondaryManagers_ManagerId",
                table: "SecondaryManagers",
                newName: "IX_SecondaryManagers_AssignedByAdminId");

            migrationBuilder.AddColumn<int>(
                name: "SecondaryManagerId",
                table: "SecondaryManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "SecondaryManagers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecondaryManagers",
                table: "SecondaryManagers",
                columns: new[] { "EmployeeId", "SecondaryManagerId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryManagers_SecondaryManagerId",
                table: "SecondaryManagers",
                column: "SecondaryManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecondaryManagers_Users_AssignedByAdminId",
                table: "SecondaryManagers",
                column: "AssignedByAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecondaryManagers_Users_SecondaryManagerId",
                table: "SecondaryManagers",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
