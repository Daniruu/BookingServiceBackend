using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingServiceBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChangedBusinessAndEmoloyeeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Services_EmployeeId",
                table: "Services",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BusinessId",
                table: "Employees",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Businesses_BusinessId",
                table: "Employees",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Employees_EmployeeId",
                table: "Services",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Businesses_BusinessId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Employees_EmployeeId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_EmployeeId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BusinessId",
                table: "Employees");
        }
    }
}
