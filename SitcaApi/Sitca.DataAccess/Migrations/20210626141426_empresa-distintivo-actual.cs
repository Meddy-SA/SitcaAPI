using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class empresadistintivoactual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResultadoActual",
                table: "Empresa",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoSugerido",
                table: "Empresa",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResultadoVencimiento",
                table: "Empresa",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Importancia",
                table: "Distintivo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultadoActual",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "ResultadoSugerido",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "ResultadoVencimiento",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "Importancia",
                table: "Distintivo");
        }
    }
}
