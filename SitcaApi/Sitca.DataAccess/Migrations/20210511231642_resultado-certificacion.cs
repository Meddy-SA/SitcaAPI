using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class resultadocertificacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "ProcesoCertificacion",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultadoId",
                table: "ProcesoCertificacion",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResultadoCertificacion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Aprobado = table.Column<bool>(nullable: false),
                    Observaciones = table.Column<string>(maxLength: 500, nullable: true),
                    DistintivoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoCertificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadoCertificacion_Distintivo_DistintivoId",
                        column: x => x.DistintivoId,
                        principalTable: "Distintivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_ResultadoId",
                table: "ProcesoCertificacion",
                column: "ResultadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoCertificacion_DistintivoId",
                table: "ResultadoCertificacion",
                column: "DistintivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_ResultadoCertificacion_ResultadoId",
                table: "ProcesoCertificacion",
                column: "ResultadoId",
                principalTable: "ResultadoCertificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_ResultadoCertificacion_ResultadoId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropTable(
                name: "ResultadoCertificacion");

            migrationBuilder.DropIndex(
                name: "IX_ProcesoCertificacion_ResultadoId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "ResultadoId",
                table: "ProcesoCertificacion");
        }
    }
}
