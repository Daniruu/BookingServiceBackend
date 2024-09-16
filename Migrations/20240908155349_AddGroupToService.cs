using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingServiceBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "Services");
        }
    }
}
