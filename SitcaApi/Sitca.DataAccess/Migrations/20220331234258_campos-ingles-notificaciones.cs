using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class camposinglesnotificaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextoInternoEn",
                table: "Notificacion",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextoParaEmpresaEn",
                table: "Notificacion",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloInternoEn",
                table: "Notificacion",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloParaEmpresaEn",
                table: "Notificacion",
                maxLength: 150,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextoInternoEn",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TextoParaEmpresaEn",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TituloInternoEn",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TituloParaEmpresaEn",
                table: "Notificacion");
        }
    }
}
