using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class NumeroExpProcesocertyestadoempresa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroExpediente",
                table: "ProcesoCertificacion",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Empresa",
                maxLength: 75,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroExpediente",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Empresa");
        }
    }
}
