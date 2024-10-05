using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class CamposBDIngles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                table: "Tipologia",
                maxLength: 75,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                table: "SubtituloSeccion",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                table: "SeccionModulo",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Pregunta",
                maxLength: 2500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEnglish",
                table: "Tipologia");

            migrationBuilder.DropColumn(
                name: "NameEnglish",
                table: "SubtituloSeccion");

            migrationBuilder.DropColumn(
                name: "NameEnglish",
                table: "SeccionModulo");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Pregunta");
        }
    }
}
