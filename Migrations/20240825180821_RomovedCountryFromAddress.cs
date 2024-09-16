using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingServiceBackend.Migrations
{
    /// <inheritdoc />
    public partial class RomovedCountryFromAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Businesses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Businesses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
