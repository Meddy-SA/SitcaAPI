using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class modulo_campo_ingles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Modulo",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Modulo");
        }
    }
}
