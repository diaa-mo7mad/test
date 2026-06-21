using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARGI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LightIntensity",
                table: "SensorReadings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RainState",
                table: "SensorReadings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeatDaily",
                table: "IrrigationSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LightIntensity",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "RainState",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "IsRepeatDaily",
                table: "IrrigationSchedules");
        }
    }
}
