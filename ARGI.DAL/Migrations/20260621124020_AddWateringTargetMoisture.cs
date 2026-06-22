using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARGI.DAL.Migrations
{
        public partial class AddWateringTargetMoisture : Migration
    {
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WateringTargetMoisture",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

                protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WateringTargetMoisture",
                table: "Domes");
        }
    }
}
