using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class capacitaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capacitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaCarga = table.Column<DateTime>(nullable: false),
                    Nombre = table.Column<string>(maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(maxLength: 250, nullable: true),
                    Ruta = table.Column<string>(maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(maxLength: 10, nullable: true),
                    Activo = table.Column<bool>(nullable: false),
                    UsuarioCargaId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capacitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Capacitaciones_AspNetUsers_UsuarioCargaId",
                        column: x => x.UsuarioCargaId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Capacitaciones_UsuarioCargaId",
                table: "Capacitaciones",
                column: "UsuarioCargaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Capacitaciones");
        }
    }
}
