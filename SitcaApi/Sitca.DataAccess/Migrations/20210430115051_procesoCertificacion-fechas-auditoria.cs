using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class procesoCertificacionfechasauditoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFijadaAuditoria",
                table: "ProcesoCertificacion",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSolicitudAuditoria",
                table: "ProcesoCertificacion",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFijadaAuditoria",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "FechaSolicitudAuditoria",
                table: "ProcesoCertificacion");
        }
    }
}
