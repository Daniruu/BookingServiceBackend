using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookingServiceBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessToWotkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkingHours",
                table: "WorkingHours");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "WorkingHours",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkingHours",
                table: "WorkingHours",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_BusinessId",
                table: "WorkingHours",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkingHours",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_BusinessId",
                table: "WorkingHours");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "WorkingHours",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkingHours",
                table: "WorkingHours",
                columns: new[] { "BusinessId", "Day" });
        }
    }
}
