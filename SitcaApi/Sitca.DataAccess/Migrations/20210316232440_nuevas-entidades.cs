using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class nuevasentidades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuestionario",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresa = table.Column<int>(nullable: false),
                    FechaInicio = table.Column<DateTime>(nullable: false),
                    FechaGenerado = table.Column<DateTime>(nullable: false),
                    FechaFinalizado = table.Column<DateTime>(nullable: false),
                    TipologiaId = table.Column<int>(nullable: true),
                    IdTipologia = table.Column<int>(nullable: false),
                    IdAuditor = table.Column<int>(nullable: false),
                    IdAsesor = table.Column<int>(nullable: false),
                    Resultado = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuestionario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cuestionario_Tipologia_TipologiaId",
                        column: x => x.TipologiaId,
                        principalTable: "Tipologia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cuestionario_TipologiaId",
                table: "Cuestionario",
                column: "TipologiaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cuestionario");
        }
    }
}
