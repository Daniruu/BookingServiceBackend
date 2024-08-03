﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingServiceBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Businesses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Businesses");
        }
    }
}
