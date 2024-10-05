using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class procesocertificacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcesoCertificacion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaInicio = table.Column<DateTime>(nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: true),
                    AsesorId = table.Column<string>(maxLength: 450, nullable: true),
                    AuditorId = table.Column<string>(maxLength: 450, nullable: true),
                    EmpresaId = table.Column<int>(nullable: false),
                    UserGeneraId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesoCertificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesoCertificacion_AspNetUsers_AsesorId",
                        column: x => x.AsesorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcesoCertificacion_AspNetUsers_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcesoCertificacion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcesoCertificacion_AspNetUsers_UserGeneraId",
                        column: x => x.UserGeneraId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_AsesorId",
                table: "ProcesoCertificacion",
                column: "AsesorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_AuditorId",
                table: "ProcesoCertificacion",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_EmpresaId",
                table: "ProcesoCertificacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_UserGeneraId",
                table: "ProcesoCertificacion",
                column: "UserGeneraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcesoCertificacion");
        }
    }
}
