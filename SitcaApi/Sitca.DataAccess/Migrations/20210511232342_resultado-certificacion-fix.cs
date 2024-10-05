using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class resultadocertificacionfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_ResultadoCertificacion_ResultadoId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropIndex(
                name: "IX_ProcesoCertificacion_ResultadoId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "ResultadoId",
                table: "ProcesoCertificacion");

            migrationBuilder.AddColumn<int>(
                name: "CertificacionId",
                table: "ResultadoCertificacion",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoCertificacion_CertificacionId",
                table: "ResultadoCertificacion",
                column: "CertificacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultadoCertificacion_ProcesoCertificacion_CertificacionId",
                table: "ResultadoCertificacion",
                column: "CertificacionId",
                principalTable: "ProcesoCertificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultadoCertificacion_ProcesoCertificacion_CertificacionId",
                table: "ResultadoCertificacion");

            migrationBuilder.DropIndex(
                name: "IX_ResultadoCertificacion_CertificacionId",
                table: "ResultadoCertificacion");

            migrationBuilder.DropColumn(
                name: "CertificacionId",
                table: "ResultadoCertificacion");

            migrationBuilder.AddColumn<int>(
                name: "ResultadoId",
                table: "ProcesoCertificacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_ResultadoId",
                table: "ProcesoCertificacion",
                column: "ResultadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_ResultadoCertificacion_ResultadoId",
                table: "ProcesoCertificacion",
                column: "ResultadoId",
                principalTable: "ResultadoCertificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
