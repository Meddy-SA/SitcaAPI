using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class homologacion2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Homologacion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Distintivo = table.Column<string>(maxLength: 70, nullable: true),
                    DatosProceso = table.Column<string>(maxLength: 1000, nullable: true),
                    FechaOtorgamiento = table.Column<DateTime>(nullable: false),
                    FechaVencimiento = table.Column<DateTime>(nullable: false),
                    EmpresaId = table.Column<int>(nullable: false),
                    CertificacionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homologacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Homologacion_ProcesoCertificacion_CertificacionId",
                        column: x => x.CertificacionId,
                        principalTable: "ProcesoCertificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Homologacion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Homologacion_CertificacionId",
                table: "Homologacion",
                column: "CertificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Homologacion_EmpresaId",
                table: "Homologacion",
                column: "EmpresaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Homologacion");
        }
    }
}
