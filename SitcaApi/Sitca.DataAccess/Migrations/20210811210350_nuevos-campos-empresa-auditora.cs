using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class nuevoscamposempresaauditora : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinConcesion",
                table: "CompAuditoras",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioConcesion",
                table: "CompAuditoras",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCertificado",
                table: "CompAuditoras",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Representante",
                table: "CompAuditoras",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "CompAuditoras",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFinConcesion",
                table: "CompAuditoras");

            migrationBuilder.DropColumn(
                name: "FechaInicioConcesion",
                table: "CompAuditoras");

            migrationBuilder.DropColumn(
                name: "NumeroCertificado",
                table: "CompAuditoras");

            migrationBuilder.DropColumn(
                name: "Representante",
                table: "CompAuditoras");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "CompAuditoras");
        }
    }
}
