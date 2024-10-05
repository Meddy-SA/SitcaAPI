using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class observacionespreguntasyactivitylog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(nullable: false),
                    User = table.Column<string>(nullable: true),
                    Observaciones = table.Column<string>(maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuestionarioItemObservaciones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(nullable: false),
                    Observaciones = table.Column<string>(maxLength: 1000, nullable: true),
                    CuestionarioItemId = table.Column<int>(nullable: false),
                    UsuarioCargaId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuestionarioItemObservaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuestionarioItemObservaciones_CuestionarioItem_CuestionarioItemId",
                        column: x => x.CuestionarioItemId,
                        principalTable: "CuestionarioItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuestionarioItemObservaciones_CuestionarioItemId",
                table: "CuestionarioItemObservaciones",
                column: "CuestionarioItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLog");

            migrationBuilder.DropTable(
                name: "CuestionarioItemObservaciones");
        }
    }
}
