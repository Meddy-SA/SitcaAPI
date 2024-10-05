using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class cuestionarioArchivo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "ProcesoCertificacion",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuestionarioItemId",
                table: "Archivo",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_TipologiaId",
                table: "ProcesoCertificacion",
                column: "TipologiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_CuestionarioItemId",
                table: "Archivo",
                column: "CuestionarioItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Archivo_CuestionarioItem_CuestionarioItemId",
                table: "Archivo",
                column: "CuestionarioItemId",
                principalTable: "CuestionarioItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_Tipologia_TipologiaId",
                table: "ProcesoCertificacion",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Archivo_CuestionarioItem_CuestionarioItemId",
                table: "Archivo");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_Tipologia_TipologiaId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropIndex(
                name: "IX_ProcesoCertificacion_TipologiaId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropIndex(
                name: "IX_Archivo_CuestionarioItemId",
                table: "Archivo");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "CuestionarioItemId",
                table: "Archivo");
        }
    }
}
