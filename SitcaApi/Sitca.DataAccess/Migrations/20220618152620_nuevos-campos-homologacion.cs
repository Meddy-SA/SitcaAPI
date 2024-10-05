using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class nuevoscamposhomologacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistintivoExterno",
                table: "Homologacion",
                maxLength: 70,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnProcesoSiccs",
                table: "Homologacion",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Homologacion",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaEdicion",
                table: "Homologacion",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistintivoExterno",
                table: "Homologacion");

            migrationBuilder.DropColumn(
                name: "EnProcesoSiccs",
                table: "Homologacion");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Homologacion");

            migrationBuilder.DropColumn(
                name: "FechaUltimaEdicion",
                table: "Homologacion");
        }
    }
}
