using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARGI.DAL.Migrations
{
        public partial class changename : Migration
    {
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "FullName");
        }

                protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "FirstName");
        }
    }
}
