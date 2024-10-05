using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class agregacamposuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoIdentidad",
                table: "AspNetUsers",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nacionalidad",
                table: "AspNetUsers",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profesion",
                table: "AspNetUsers",
                maxLength: 120,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoIdentidad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nacionalidad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Profesion",
                table: "AspNetUsers");
        }
    }
}
