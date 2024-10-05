using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class cuestionarioItemHistoryyfkcerticuestionario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcesoCertificacionId",
                table: "Cuestionario",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CuestionarioItemHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(nullable: false),
                    Item = table.Column<string>(maxLength: 30, nullable: true),
                    Type = table.Column<string>(maxLength: 20, nullable: true),
                    Result = table.Column<int>(nullable: false),
                    CuestionarioItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuestionarioItemHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuestionarioItemHistories_CuestionarioItem_CuestionarioItemId",
                        column: x => x.CuestionarioItemId,
                        principalTable: "CuestionarioItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cuestionario_ProcesoCertificacionId",
                table: "Cuestionario",
                column: "ProcesoCertificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuestionarioItemHistories_CuestionarioItemId",
                table: "CuestionarioItemHistories",
                column: "CuestionarioItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cuestionario_ProcesoCertificacion_ProcesoCertificacionId",
                table: "Cuestionario",
                column: "ProcesoCertificacionId",
                principalTable: "ProcesoCertificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cuestionario_ProcesoCertificacion_ProcesoCertificacionId",
                table: "Cuestionario");

            migrationBuilder.DropTable(
                name: "CuestionarioItemHistories");

            migrationBuilder.DropIndex(
                name: "IX_Cuestionario_ProcesoCertificacionId",
                table: "Cuestionario");

            migrationBuilder.DropColumn(
                name: "ProcesoCertificacionId",
                table: "Cuestionario");
        }
    }
}
