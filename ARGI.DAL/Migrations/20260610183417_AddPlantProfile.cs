using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARGI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPlantProfileCalibrated",
                table: "Domes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "OptimalLightMax",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OptimalLightMin",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OptimalMoistureMax",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OptimalMoistureMin",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OptimalTempMax",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OptimalTempMin",
                table: "Domes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPlantProfileCalibrated",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalLightMax",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalLightMin",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalMoistureMax",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalMoistureMin",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalTempMax",
                table: "Domes");

            migrationBuilder.DropColumn(
                name: "OptimalTempMin",
                table: "Domes");
        }
    }
}
