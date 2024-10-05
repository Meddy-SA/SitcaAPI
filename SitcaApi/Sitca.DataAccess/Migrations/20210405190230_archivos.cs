using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class archivos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archivo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaCarga = table.Column<DateTime>(nullable: false),
                    Nombre = table.Column<string>(maxLength: 100, nullable: true),
                    Ruta = table.Column<string>(maxLength: 50, nullable: true),
                    Tipo = table.Column<string>(maxLength: 10, nullable: true),
                    EmpresaId = table.Column<int>(nullable: true),
                    UsuarioCargaId = table.Column<string>(maxLength: 450, nullable: true),
                    UsuarioId = table.Column<string>(maxLength: 450, nullable: true),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Archivo_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Archivo_AspNetUsers_UsuarioCargaId",
                        column: x => x.UsuarioCargaId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Archivo_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_EmpresaId",
                table: "Archivo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_UsuarioCargaId",
                table: "Archivo",
                column: "UsuarioCargaId");

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_UsuarioId",
                table: "Archivo",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archivo");
        }
    }
}
