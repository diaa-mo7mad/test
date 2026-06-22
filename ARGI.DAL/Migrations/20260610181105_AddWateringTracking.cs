using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARGI.DAL.Migrations
{
        public partial class AddWateringTracking : Migration
    {
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastWateredAt",
                table: "Domes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WateringSource",
                table: "Domes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

                protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWateredAt",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "WateringSource",
                table: "Domes");
        }
    }
}
